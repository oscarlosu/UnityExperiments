-------------------------------------------------------------------------------------------------
                                          Gif Creator
                                         Version 1.0.7
                                       PygmyMonkey Tools
                                     tools@pygmymonkey.com
                   	       http://pygmymonkey.com/tools/gif-creator/
-------------------------------------------------------------------------------------------------
Thank you for buying Gif Creator!

If you have questions, suggestions, comments or feature requests, please send us an email
at tools@pygmymonkey.com



-------------------------------------------------------------------------------------------------
                            Support, Documentation, Examples and FAQ
-------------------------------------------------------------------------------------------------
You can find everything at http://pygmymonkey.com/tools/gif-creator/



-------------------------------------------------------------------------------------------------
                                  How to update Gif Creator
-------------------------------------------------------------------------------------------------
1. Close the Gif Creator window
2. Delete everything under the 'GifCreator' folder from the Project View
3. Import the latest version from the Asset Store



-------------------------------------------------------------------------------------------------
                                           Get Started
-------------------------------------------------------------------------------------------------
Gif Creator allows you to record a gif from a camera, or the entire game view, directly inside Unity.
You can launch the interface by going to "Window/PygmyMonkey/Gif Creator" in Unity.


---------------------------------------- Camera section -----------------------------------------
The very first thing you need to do, is decide what you want to record. There is two methods to
create your gifs:

- Record single camera (the fastest method):
You just need to drag and drop your camera on the camera field, and it will record each frame
coming from what this exact camera sees.

- Record everything (the slowest method):
But what if you have multiple cameras? Or if you have a canvas for your UI that is set to 'Screen
Space - Overlay' ? Or if the canvases you're using are rendered by another camera?
Well, there is no fast way to get each frame from Unity in these cases, and we'll need to rely on
a method that is slow... that is, retrieving each frame, the entire screen pixel array.
In order to use the slowest method, you just need to toggle the 'Record everything' tab.
WARNING: Also, due to how the method to capture the screen works in Unity, your game should take
the ENTIRE space available in the game view. It means you must have either the game view set to
'Free Aspect', or you have to resize the game view so it fits the specific resolution you specified
for the game view. Otherwise, it will record the dark gray background.

If you want to create a gif from a high consuming frame rate project, the slowest method might
not work really well, and will greatly impact your frame rate. But if you have a small project,
like a 2D game, or a game where you don't really need to have a high frame rate, it can work
really great :)


-------------------------------------- Parameters section ---------------------------------------
This section allows you to specify all the parameters for the recording.
Once you hit the record button, you can't modify these values anymore, you'll need to press the
'Reset' button to start a new recording (more on this later).

--- Screen section ---
You can choose between 3 different screen sections:

- Fullscreen:
This will record the entire screen of your game.
You can see that the positionX, positionY, width and height values can't be edited in this section.

- Custom Rect Absolute:
Here you can decide which part of the screen you want to record, providing a bottom left position
(using the positionX and positionY variables), and the width and height.
These are absolute numbers, so if you change your game view size, you will not record the same
part of the screen.

- Custom Rect Relative:
Same thing than Rect Absolute, but here you specify values in a [0, 1] range.
This allows you to be completely screen size independent and always get the same part of the screen.

For the Custom Rect sections, you can toggle the 'Preview on screen' toggle to see exactly which
part of the screen will be recorded. And you can change the color of the preview rectangle.
When you'll hit the Record button, the 'Preview on screen' toggle will turn to false so you don't
record your red preview rectangle when making the gif :) But even after that you can toggle it
whenever you want to see it and adjust it.

--- Frames Per Seconds ---
You can define here, how many frames per seconds you want to record.
Of course, the more frames you have, the bigger the gif will be, so adjust according to your needs :)

--- Duration ---
Here you specify the number of seconds you want to record.

--- Record width ---
Let's say your screen size is 1280x720. It's a pretty big size for a gif, so maybe you want to
reduce that to a smaller size so that the gif is not too big and can be easily viewed by everyone.
In order to do that, you just need to specify the smallest width you can think of. Maybe 320px?
The height will be automatically calculated, based on the screen section you defined earlier.

--- Estimated memory used ---
At the end of the parameters section, you'll see some information on the estimated memory that
will be used to record all the frames for your gif.
So if you have a big record width, a long duration and a lot of frame per seconds, this can take
a significant size in the memory of your computer.


---------------------------------------- Record section -----------------------------------------
This section is where you have control on the recording.
You can only start the recording when you're in play mode. And when you're done recording, you
should stay in play mode until you are done with saving and/or exporting your gif.

--- Record type ---
You can chose between two different types of recording:

- Duration:
Here, Gif Creator will simply record the duration you specified in the parameters section.
You can also stop the recording, whenever you want pressing the 'Stop Record' button.

- Last Seconds:
With this mode, Gif Creator will continuously record and only keep the x last seconds.
x is the duration you specified in the parameters section.
When you want to stop the record, simply press the 'Stop Record' button.

--- The Reset button ---
Once you've recorded something, if you want to record something else, you need to press this
'Reset' button. This will delete the last frames you've recorded and allow you to start all over.


---------------------------------------- Preview section ----------------------------------------
Once you're done recording, you'll be able to preview the frames in the preview section.
You have basic controls under the preview: Play/Pause, Stop, Set Frame Start and Set Frame End.
Here, you can choose to record a specific part of your gif and not all the frames.

--- Frame Index ---
With the frame index slider, you can easily navigate between all the frames you've recorded, and
see the result in the preview.

--- Frames ---
Here, you can use this min/max slider to define the start and end of your gif.

--- Frame start & Frame end ---
If you want to be more precise, you can simply set the frame start & frame end here instead of
using the min/max slider.
Once they're set, you can just press play to see only the part you've selected.

--- Frame interval selected ---
You'll see here some info about the number of frames you're about to save.


---------------------------------------- Export section -----------------------------------------
In this section, you'll be able to save or upload your gif to imgur.com!

--- Repeat ---
Based on the repeat value, you can decide if :
- You don't want it to loop (-1)
- You want it to loop forever (0)
- Or you want it to loop only a certain amount of time (n)

--- Compression Quality ---
Sets quality of color quantization (conversion of images to the maximum 256 colors allowed by the
GIF specification).
Lower values (minimum = 1) produce better colors, but slow processing significantly.
Higher values will speed up the quantization pass at the cost of lower image quality (maximum = 100).

--- Output width ---
You can define the final output width of the gif file. The best thing to do, is keep the same width
you used for the record width you defined earlier in the parameters sections.
If you specify a smaller or bigger width, a bilinear resize will be applied to the gif. This could
degrade the quality of your gif depending on the value you specify.
But of course, you might want to specify a smaller width to reduce the final size of your gif!

--- Final output dimension ---
You'll see here the exact final dimension of your gif file (again, the height is automatically
calculated based on the width and the aspect ratio).

--- Thread priority ---
Here you can define the thread priority to be used when processing the frames to the gif file.

--- Destination folder ---
You can define here where you want to export the gif files you save. It can be anywhere you want,
even outside your project folder.

--- Save button ---
Pressing this button will start saving your gif to the destination folder.
Depending on the different parameters you specified, saving can take some time... For example, if
you decide to save a 1280x720 gif, at 24 frame per seconds, over 30 seconds... it can take a LOT
of time...

--- Upload to imgur.com ---
Pressing this button, will first save your gif to a tmp folder (if it has not already been saved)
and then start the upload.
The upload can take a few minutes depending on the output file size, so wait until it's done :)
Once the upload is done, you'll have a message in the console with the URL (the URL will also
be copied in your clipboard).

(The tmp folder is located in Application.persistentDataPath:
Mac: /Users/USERNAME/Library/Application Support/COMPANY_NAME/PROJECT_NAME/
Windows: C:\Users\USERNAME\AppData\LocalLow\COMPANY_NAME\PROJECT_NAME\)



-------------------------------------------------------------------------------------------------
                                               Demo
-------------------------------------------------------------------------------------------------
You can find the demo scene in "PygmyMonkey/GifCreator/Demo/".
This is just a simple demo scene with some animated cubes you can use to test Gif Creator.


-------------------------------------------------------------------------------------------------
                                          Release Notes
-------------------------------------------------------------------------------------------------
1.0.7
- FIX: Every script is now in the PygmyMonkey.GifCreator namespace

1.0.6
- NEW: Added a 'Record Upside Down' toggle, when using the 'Record single camera' mode (if you
still have issues with upside down recording).

1.0.5
- FIX: Inverted image on Unity Editor Windows
- FIX: Weird preview texture on Unity Editor Windows
- NEW: Added duration info in the preview section
- NEW: Added a toggle to open the exported gif folder after saving is done

1.0.0
- NEW: Initial release


-------------------------------------------------------------------------------------------------
                                               FAQ
-------------------------------------------------------------------------------------------------

- How can I help?
Thank you! You can take a few seconds and rate the tool in the Asset Store and leave a nice
comment, that would help a lot ;)

- What's the minimum Unity version required?
Gif Creator will work starting with Unity 5.0.0.


-------------------------------------------------------------------------------------------------
                                           Other tools
-------------------------------------------------------------------------------------------------

--- Advanced Builder (http://u3d.as/6ab) ---
Advanced Builder provides an easy way to manage multiple versions of your game on a lot of
platforms. For example, with one click, Advanced Builder will build a Demo and Paid version of
your game on 4 different platforms (that's 8 builds in one click).

--- Color Palette (http://u3d.as/cbR) ---
Color Palette will help you manage all your color palettes directly inside Unity!.
Instead of manually setting each color from the color picker, you can just pick the color you
want from the Color Palette Window. You can even apply an entire palette on all the objects in
your scene with just one click.

Thanks a lot to Thomas Hourdel (@chman), who helped a lot on this tool!
Check out his awesome assets: https://www.assetstore.unity3d.com/en/#!/publisher/1627/