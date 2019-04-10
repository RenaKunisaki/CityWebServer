class HeightMap {
    constructor(app) {
        this.app = app;
        this.scale = 1.0;
        this.position = [200, 0];
        this._isFirstRefresh = true;
    }

    run() {
        this.app.registerMessageHandler("CityInfo",
            (data) => { this.refresh() });

        this._isDragging = false;
        this._dragStart  = [0, 0];
        $("#map").on({
            "mousewheel DOMMouseScroll": event => {
                let x = event.originalEvent.deltaX;
                let y = event.originalEvent.deltaY;
                this.scale -= y / 3000;
                console.log("Scrolling", x, y, "Scale", this.scale);
                event.preventDefault();
                this._redraw();
            },
            "mousedown": event => {
                if(event.button != 0) return; //only left button
                this._isDragging = true;
                this._dragStart  = [
                    this.position[0] + event.pageX,
                    this.position[1] + event.pageY,
                ];
                event.preventDefault();
            },
            "mouseup": event => {
                if(event.button != 0) return;
                this._isDragging = false;
                event.preventDefault();
            },
            "mousemove": event => {
                if(!this._isDragging) return;
                this.position[0] = this._dragStart[0] - event.pageX;
                this.position[1] = this._dragStart[1] - event.pageY;
                event.preventDefault();
                this._redraw();
            },
        });
        $('#map-zoom-to-unlocked').on('click', event => {
            this.zoomToUnlockedRegion();
        })
        this.refresh();
    }

    zoomToUnlockedRegion() {
        const width  = this._maxUnlockedX - this._minUnlockedX;
        const height = this._maxUnlockedY - this._minUnlockedY;
        const bodyW  = this.body.width();
        const bodyH =  this.body.height()
        //XXX double check this.
        if(width > height) this.scale = bodyW / width;
        else this.scale = bodyH / height;
        this.position[0] = this._minUnlockedX * this.scale;
        this.position[1] = this._minUnlockedY * this.scale;
        this._redraw();
    }

    refresh() {
        //jQuery doesn't support getting binary blobs.
        const oReq = new XMLHttpRequest();
        oReq.open("GET", "/HeightMap", true);
        oReq.responseType = "arraybuffer";
        oReq.onload = (event) => {
            console.log("Heightmap loaded.")
            const bytes = new Uint8Array(oReq.response);
            this.update(bytes);
        };
        oReq.send();
    }

    update(bytes) {
        const limits = this.app.data.Limits;
        if(!limits) {
            //XXX use await
            setTimeout(() => {this.update(bytes)}, 500);
            return;
        }
        const resolution = limits.TerrainManager.RAW_RESOLUTION + 1; //XXX why +1?
        const canvas = $('#map canvas')[0];
        const ctx    = canvas.getContext('2d');
        ctx.canvas.width  = resolution;
        ctx.canvas.height = resolution;
        const image  = ctx.createImageData(resolution, resolution);
        let   offset = 0;
        for(let y=0; y<resolution; y++) {
            for(let x=0; x<resolution; x++) {
                //the map is 16-bit but canvas only supports 8-bit images.
                //easiest to just throw away the low byte.
                //let pixel = (bytes[offset+1] << 8) | bytes[offset];
                let pixel = bytes[offset+1];
                offset += 2;
                let dest = ((y*resolution)+x)*4;
                image.data[dest  ] = 0; //red
                image.data[dest+1] = pixel; //green
                image.data[dest+2] = 0; //blue
                image.data[dest+3] = 255; //alpha
            }
        }

        this.body = $("#map .body");
        this.canvas = canvas;
        this.ctx = ctx;
        this.resolution = resolution;

        //Create the bitmap, and when it's ready, draw it.
        createImageBitmap(image).then(bitmap => {
            this.bitmap = bitmap;
            this._redraw();
        })
    }

    _redraw() {
        /** Set transformation matrix to:
         *  a c e
         *  b d f
         *  0 0 1
         *  by setTransform(a, b, c, d, e, f)
         *  a = horizontal scale, d = vertical scale
         *  e = horizontal translate, f = vertical translate
         *  We scale the Y axis negative to flip the image.
         */
        this.ctx.setTransform(1, 0, 0, 1, 0, 0); //clear first...
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        this.ctx.setTransform(
            this.scale, 0, 0,
            -this.scale, -this.position[0],
            -this.position[1] + (this.bitmap.height * this.scale));
        //console.log("Bitmap size:", this.bitmap.width, this.bitmap.height);

        this.ctx.drawImage(this.bitmap, 0, 0);
        if(this.app.data.CityInfo) this.updateLockedTiles(this.app.data.CityInfo);

        if(this._isFirstRefresh) {
            this._isFirstRefresh = false;
            this.zoomToUnlockedRegion();
        }
    }

    updateLockedTiles(cityInfo) {
        if(!this.ctx) return;
        this._minUnlockedX =  99999999;
        this._maxUnlockedX = -99999999;
        this._minUnlockedY =  99999999;
        this._maxUnlockedY = -99999999;

        let numTiles = cityInfo.isTileUnlocked.length;
        let gridSize = Math.sqrt(numTiles);
        let tileSize = Math.floor(this.resolution / gridSize);
        //console.log("grid size", gridSize, "tile size", tileSize);
        this.ctx.fillStyle = 'rgba(255, 0, 0, 0.25)';
        this.ctx.strokeStyle = "#FFF";
        let tile = 0;
        for(let y=0; y<gridSize; y++) {
            for(let x=0; x<gridSize; x++) {
                const tx = x * tileSize, ty = y * tileSize;
                const tx2 = tx+tileSize, ty2 = ty+tileSize;
                if(!cityInfo.isTileUnlocked[tile]) {
                    this.ctx.fillRect(tx, ty, tileSize, tileSize);
                }
                else {
                    if(tx  < this._minUnlockedX) this._minUnlockedX = tx;
                    if(tx2 > this._maxUnlockedX) this._maxUnlockedX = tx2;
                    if(ty  < this._minUnlockedY) this._minUnlockedY = ty;
                    if(ty2 > this._maxUnlockedY) this._maxUnlockedY = ty2;
                }
                tile++;
            }
        }
        //this.ctx.strokeRect(this._minUnlockedX, this._minUnlockedY,
        //    this._maxUnlockedX - this._minUnlockedX,
        //    this._maxUnlockedY - this._minUnlockedY);
    }
}
