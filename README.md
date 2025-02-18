# VrcLeash

This is a module for VRCOSC and a Unity component that allows you to add pullable functionality to VRC leashes.

If you're looking for OSC Leash compatibility, check out either [CrookedToe's VRCOSC Module](https://github.com/CrookedToe/CrookedToe-s-Modules) or the original [Zenith's Original OSCLeash](https://github.com/ZenithVal/OSCLeash).

There is beta support for leashes built using OSC Leash, but I recommend the above for stability if that's your only need. Especially CrookedToe's for convenience and vertical playspace support.

## Setup

VrcLeash requires two parts to work. First, you need to add the necessary Unity component to your Avatar's leash. This adds the necessary OSC parameters and contacts that allow VrcLeash to track the position and pull of your leash.
Then, you need to setup the VRCOSC module. This tracks the above components of the leash in game, and communicates back VRChat the necessary movements.

### Setting up the Leash in Unity

Any leash object that utilizes a Physbone is compatible with this component.

To add VrcLeash to you leash, drop the `VrcLeash.unitypackage` into your project. Then add the Vrc Leash component to the root of your leash object. Clicking "Auto Setup" will default the leash name to Leash, and automatically locate the Physbone for the leash and wire up the necessary contacts to make the leash work if the PhysBone is a child of your leash Game Object.

If Auto Setup doesn't work, or it picks the wrong parts of you leash, you can manually select your leash's PhysBone and the "end" of your leashes armeture.

You'll want to modify and record the leash's Stretch value. You'll move depending on how much the leash is "stretched", so you'll want to make sure the Leash is stretchable and that the value of the stretch is set to something that looks good when fully stretched.

In addition, you'll want to make sure that you're leash is attached to a relative stable part of your body. If it's attached to a part of your armeture that rocks a lot during your movement, this can cause the leash itself to move and cause erratic movement. You might also want to potentially set the "Leash End" to a shorter part of your leash if the leash might be grabbed by the middle portion.

### Setting up VRCOSC

The OSC control is and "pull" (e.g. movement) is powered by the [VRCOSC](https://github.com/VolcanicArts/VRCOSC) toolkit. Download and install that program, and then install the VRC Pullable Leash module. 

Then go into the `VRC Leash` settings and add a Leash. This will default to to mostly sane values, and you'll want to modify these to find what feels right. **Most importantly,** you should set the Running and Walking Deadzone values based on the values from the Debug info of the Unity Component above, as these control when the leash's "Stretch" value will start pulling you.

Then it should all work! You can test by "pulling" your own Leash, and you can tweak the VRC Leash settings at anytime while in VRChat to tweak the values.


## TODOs

* Incorporate the Vertical Playspace support from CrookedToe's module
* Better document Leash setup and quirks
* Build and document full support for original OSC Leash prefabs since many assets are sold with support for this.
