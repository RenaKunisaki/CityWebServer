# CityWebServer
Adds a web server to Cities: Skylines, allowing you to view live game information in a web browser.

Enable the mod, start the game, and open a browser to http://127.0.0.1:7135/ to see the server in action. It will display a lot of charts and information.

# Work in progress!

May contain many bugs! But should not harm the game at all.

## Is it safe?

Yes. It doesn't touch the game's save data, and only calls public API methods, so it should never be able to break the game.

## Is it secure?

Mostly. It does provide a debug interface that can probably be leveraged to run arbitrary code in the game process. So, another program running on the same computer could take over the game.
Right now, it doesn't accept connections from other computers. That will change in the future (and the debug interface probably removed).

## Is it cheating?

Only if you use it to cheat. It will let you break some rules (demolishing non-empty buildings) but primarily it's just a convenient interface to show game info on a second monitor, so that you aren't going years without realizing a police station burned down and never got rebuilt.

## Does it affect performance?

The performance impact should be little to none, but running a web browser alongside the game might take up a bunch of extra RAM and CPU. (But, Steam's interface is already a web browser, so maybe not a big deal.) It uses very little memory, and is idle most of the time.

# Overview

- Top bar: Shows city name and in-game date.
    - Sun/Moon shows whether it's day/night time (in-game day cycle), if this is enabled.
- Chirper Messages: Shows recent Chirps from your citizens.
- Game Limits: Shows how close you're coming to the built-in limits of the game.
    - Don't worry too much about Citizen Instances; the game manages it automatically.
    - Currently, the server is not aware of Unlimited Trees Mod, so displays the original tree limit.
- Resources: Shows how much of various things are used/available in the city.
- Stats: Shows some of the same info as Resources, but graphed over time.
- Budget: Shows a breakdown of your incomes and expenses.
- Income/Expense Charts: Shows your budget information over time.
- Population: Shows the makeup of your population and changes over time.
- Problems: Shows how many instances of each problem there are. Click on one of the icons to display a list.
    - From the list, you can see each problem building and click Show to focus the camera on it, Demolish to destroy it, or Rebuild to rebuild it if necessary.
    - Demolish will let you destroy some buildings the game normally won't let you destroy, eg cemeteries and dumps that aren't empty. It will give the appropriate refunds.
    - Rebuild only functions if the building is destroyed, owned by the city, and ready for rebuilding. (The API is capable of overriding these restrictions, but this functionality isn't exposed in the UI yet.) It deducts the appropriate funds and fails if you don't have enough money.

Hover the mouse over points on the charts for more details.

# API

Documentation to be written... for now, look at the code.
You can connect to the WebSocket server and call methods. New information is pushed every in-game day.

# Features to come

- Display terrain map with info (traffic, problems, etc) and click to go there.
    - Display it on a tablet computer and use swipe gestures to pan the camera.
- Display per-district info.
- Display public transit info.
- Click a Citizen's name in the Chirper box to show their info.
- Show more details in budget info (income/expense per level)
- Be able to close and rearrange the boxes.
- Auto reconnect to game. (For now, you have to refresh the page.)
- Don't display erroneous info if connected before game finishes loading, or if player loads a different save mid-game.
- Grab information from game's stats panel to fill in charts with info from before the page was loaded.
- Show info about football matches.
- Notify when a rocket is ready to launch.
- Get localized strings from the game.
- Notification when a new problem arises (maybe animate the icon for a second)

# Known issues

- Building popup lists don't refresh while open.
- All Chirps received before the page was opened are given the current in-game date and time. (The game doesn't actually store their time at all.)
- UI probably breaks on some screen sizes.
- Demolish button doesn't check if the building was un-abandoned. So, there's a risk that you'll demolish a building that *was* abandoned but now isn't. (This is the same in-game, since the building can become un-abandoned just before you click.)
- Some charts could be better (eg Resources should be consistent - for some, higher is better, and for others, lower is better.)
    - Some don't like to show the full X axis.
    - Dates are shown in full at every interval.
- Some icons are probably missing; some are unclear/duplicated.

# Credits

- Built upon the original CityWebServer by Rychard.
- Uses JsonFx, jQuery, Bootstrap, and Chart.js.

![Screenshot](http://i.imgur.com/U3dD0vd.png)
(newer screenshot to come)
