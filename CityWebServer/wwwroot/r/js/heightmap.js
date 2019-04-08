class HeightMap {
    constructor(app) {
        this.app = app;
    }

    run() {
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
        const resolution = limits.TerrainManager.RAW_RESOLUTION + 1; //XXX why?
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
                //Y flip image
                let dest = (((resolution-y)*resolution)+x)*4;
                image.data[dest  ] = 0; //red
                image.data[dest+1] = pixel; //green
                image.data[dest+2] = 0; //blue
                image.data[dest+3] = 255; //alpha
            }
        }
        ctx.putImageData(image, 0, 0);
        this.image = image;
        this.ctx   = ctx;
        this.resolution = resolution;
    }

    _showTileLocked(xpos, ypos, tileSize, locked) {
        xpos *= tileSize;
        ypos *= tileSize;
        for(let y=0; y<tileSize; y++) {
            for(let x=0; x<tileSize; x++) {
                let offs = (((y+ypos)*this.image.width)+x+xpos)*4;
                this.image.data[offs] = locked ? 64 : 0; //change red
            }
        }
    }

    updateLockedTiles(cityInfo) {
        if(!this.ctx) return;
        let numTiles = cityInfo.isTileUnlocked.length;
        let gridSize = Math.sqrt(numTiles);
        let tileSize = Math.floor(this.resolution / gridSize);
        //console.log("grid size", gridSize, "tile size", tileSize);
        //this.ctx.fillStyle = 'red';  //'rgba(255, 0, 0, 0.25)';
        let tile = 0;
        for(let y=0; y<gridSize; y++) {
            //let msg = '';
            for(let x=0; x<gridSize; x++) {
                //msg += cityInfo.isTileUnlocked[tile] ? 'O ' : 'X ';
                this._showTileLocked(x, gridSize-(y+1), tileSize,
                    !cityInfo.isTileUnlocked[tile]);
                //lol too fucking easy
                //this.ctx.fillRect(x*tileSize, y*tileSize, tileSize, tileSize);
                tile++;
            }
            //console.log(msg);
        }
        this.ctx.putImageData(this.image, 0, 0);

    }
}
