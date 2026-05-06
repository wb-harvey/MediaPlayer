This is the ABX.MediaPlayer, forked from PVS.MediaPlayer (1.6), inspired by Media Foundation .NET

This LGPL library allows a .Net application to communicate directly to Windows Media Foundation to open media files (Audio and Video).

The API is identical to PVS.MediaPlayer, but this version has stability improvements.

PVS Media Player used to be a CodeProject article without source code.

ABX.MediaPlayer

(1) starts faster than LibVlcSharp

(2) seeks to the millisecond more accurately than LibVlc

(3) uses less CPU than LibVlc, probably because Windows Media Foundation is optimized for native Windows operation

(4) has built in functionality to play a clip (from->to) instead of a whole file

The included MediaTest project (vb.net 4.8) enables you to test the functionality of ABX.MediaPlayer (from source) and LibVlcSharp (from nuget).

If you are a developer, this functionality might be useful for you.

If you are a user, I highly recommend using VLC Media Player from https://videolan.org

William Harvey
junkmail@williamharvey.com
