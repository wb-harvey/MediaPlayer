/****************************************************************

    ABX.MediaPlayer - Version 1.6
    August 2021, The Netherlands
    © Copyright 2021 PVS The Netherlands - licensed under Lesser General Public License v2.1 (LGPL 2.1)

    ABX.MediaPlayer uses (part of) the Media Foundation .NET library by nowinskie and snarfle (https://sourceforge.net/projects/mfnet).
    Licensed under either Lesser General Public License v2.1 or BSD.  See license.txt or BSDL.txt for details (http://mfnet.sourceforge.net).

    ****************

    For use with Microsoft Windows 7 or higher*, Microsoft .NET Core 3.1, .NET Framework 4.x, .NET 5.0 or higher and WinForms (any CPU).
    * Use of the recorder requires Windows 8 or later.

    Created with Microsoft Visual Studio.

    Article on CodeProject with information on the use of the ABX.MediaPlayer library:
    https://www.codeproject.com/Articles/109714/PVS-MediaPlayer-Audio-and-Video-Player-Library

    ****************

    The ABX.MediaPlayer library source code is divided into 8 files:

    1. Player.cs        - main source code
    2. SubClasses.cs    - various grouping and information classes
    3. Interop.cs       - unmanaged Win32 functions
    4. AudioDevices.cs  - audio devices and peak meters
    5. DisplayClones.cs - multiple video displays 
    6. CursorHide.cs    - hides the mouse cursor during inactivity
    7. Subtitles.cs     - subrip (.srt) subtitles
    8. Infolabel.cs     - custom ToolTip

    Required references:

    System
    System.Drawing
    System.Windows.Forms

    ****************

    This file: SubClasses.cs

    Device Info Class
    Video Track Class
    Video Stream Struct
    Video Display Class
    WebCam Device Class
    Webcam Property Class
    Webcam Video Format Class
    Webcam Settings Class
    Audio Track Class
    Audio Stream Struct
    Audio Device Class
    Audio Input Device Class
    Slider Value Class
    Metadata Class
    Media Chapters Class
    Display Clone Properties Class
    OverlayForm Class
    OverlayLabel Class

    Player MF Callback Class
    Hide System Object Members Classes

    Grouping Classes Player

    ****************

    Thanks!

    Many thanks to Microsoft (Windows, .NET Framework, Visual Studio and others), all the people
    writing about programming on the internet (a great source for ideas and solving problems),
    the websites publishing those or other writings about programming, the people responding to the
    ABX.MediaPlayer articles with comments and suggestions and, of course, the people at CodeProject.

    Thanks to Google for their free online services like Search, Drive, Translate and others.

    Special thanks to the creators of Media Foundation .NET for their great library!

    Special thanks to Sean Ewington and Deeksha Shenoy of CodeProject who also took care of publishing the many
    code updates and changes in the ABX.MediaPlayer articles in a friendly, fast, and highly competent manner.
    Thank you very much, Sean and Deeksha!

    Peter Vegter
    August 2021, The Netherlands

    ****************************************************************/

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

#region Disable Some Compiler Warnings

#pragma warning disable IDE0044 // Add readonly modifier

#endregion


namespace ABX.MediaPlayer
{

    // ******************************** Device Info Class (Abstract Class)

    #region Device Info Class

    /// <summary>
    /// A class that is used as a base class for the device information classes.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class DeviceInfo : HideObjectMembers
    {
        #region Fields (Device Info Class)

        internal string _id;
        internal string _name;
        internal string _adapter;

        #endregion

        /// <summary>
        /// Gets the identifier of the device.
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Gets the description of the device.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the name of the adapter to which the device is attached.
        /// </summary>
        public string Adapter { get { return _adapter; } }

        /// <summary>
        /// Returns a string that represents this device information.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_adapter)) return _name;
            return string.Format("{0} ({1})", _name, _adapter);
        }
    }

    #endregion


    // ******************************** Video Track Class

    #region Video Track Class

    /// <summary>
    /// A class that is used to store media video track information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class VideoTrack : HideObjectMembers
    {
        #region Fields (Video Track Class)

        internal Guid   _mediaType;
        internal string _name;
        internal string _language;
        internal float  _frameRate;
        internal int    _width;
        internal int    _height;

        #endregion

        internal VideoTrack() { }

        /// <summary>
        /// Gets the media type (MF GUID) of the track (see Media Foundation documentation).
        /// </summary>
        public Guid MediaType { get { return _mediaType; } }

        /// <summary>
        /// Gets the name of the track.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the language of the track.
        /// </summary>
        public string Language { get { return _language; } }

        /// <summary>
        /// Gets the frame rate of the track.
        /// </summary>
        public float FrameRate { get { return _frameRate; } }

        /// <summary>
        /// Gets the video width of the track.
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Gets the video height of the track.
        /// </summary>
        public int Height { get { return _height; } }
    }

    #endregion


    // ******************************** Video Stream Struct

    #region Video Stream Struct

    internal struct VideoStream
    {
        internal Guid   MediaType;
        internal int    StreamIndex;
        internal bool   Selected;
        internal string Name;
        internal string Language;
        internal float  FrameRate;
        internal int    SourceWidth;
        internal int    SourceHeight;

        internal bool   PixelAspectRatio;
        internal double PixelWidthRatio;
        internal double PixelHeightRatio;

        internal bool   Rotated;
        internal int    Rotation;

        // used to show one image when video has 2 images beside each other
        //internal int    View3D; // 0 = none; 2 = left/right, 3 = top/bottom
    }

    #endregion


    // ******************************** Video Display Class

    #region Video Display Class

    internal sealed class VideoDisplay : Control
    {
		internal VideoDisplay() { SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true); }
		protected override void OnPaint(PaintEventArgs e) { }
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x0084) m.Result = (IntPtr)(-1);
			else base.WndProc(ref m);
		}
	}

    #endregion


    // ******************************** Webcam Device Class

    #region Webcam Device Class

    /// <summary>
    /// A class that is used to provide webcam device information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class WebcamDevice : DeviceInfo
    {
        internal WebcamDevice() { }
    }

    #endregion


    // ******************************** Webcam Property Info Class

    #region Webcam Property Class

    /// <summary>
    /// A class that is used to store webcam property data.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class WebcamProperty : HideObjectMembers
    {
        #region Fields (Webcam Property Class)

        internal string _name;
        internal bool   _supported;
        internal int    _min;
        internal int    _max;
        internal int    _step;
        internal int    _default;
        internal int    _value;
        internal bool   _autoSupport;
        internal bool   _auto;

        internal bool                   _isProcAmp;
        internal CameraControlProperty  _controlProp;
        internal VideoProcAmpProperty   _procAmpProp;

        #endregion

        internal WebcamProperty() { }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name
        { get { return _name; } }

        /// <summary>
        /// A value that indicates whether the property is supported by the webcam.
        /// </summary>
        public bool Supported
        {
            get { return _supported; }
        }

        /// <summary>
        /// The minimum value of the webcam property.
        /// </summary>
        public int Minimum { get { return _min; } }

        /// <summary>
        /// The maximum value of the webcam property.
        /// </summary>
        public int Maximum { get { return _max; } }

        /// <summary>
        /// The default value of the webcam property.
        /// </summary>
        public int Default { get { return _default; } }

        /// <summary>
        /// The step size for the webcam property. The step size is the smallest increment by which the webcam property can change.
        /// </summary>
        public int StepSize { get { return _step; } }

        /// <summary>
        /// Gets or sets the value of the webcam property. When set, the Automatic setting is set to false (manual control).
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _auto = false;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the property can be controlled automatically by the webcam. See also: WebcamProperty.AutoEnabled.
        /// </summary>
        public bool AutoSupport
        {
            get { return _autoSupport; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the property is controlled automatically by the webcam. See also: WebcamProperty.AutoSupported.
        /// </summary>
        public bool AutoEnabled
        {
            get { return _auto; }
            set { _auto = value; }
        }

    }

    #endregion


    // ******************************** Webcam Video Format Class

    #region Webcam Video Format Class

    /// <summary>
    /// A class that is used to store webcam video output format information.
    /// </summary>
    [CLSCompliant(true)]
    [Serializable]
    public sealed class WebcamFormat : HideObjectMembers
    {
        #region Fields (Webcam Video Format Class)

        internal int    _streamIndex;
        internal int    _typeIndex;
        internal int    _width;
        internal int    _height;
        internal float  _frameRate;

        #endregion

        internal WebcamFormat(int streamIndex, int typeIndex, int width, int height, float frameRate)
        {
            _streamIndex    = streamIndex;
            _typeIndex      = typeIndex;
            _width          = width;
            _height         = height;
            _frameRate      = frameRate;
        }

        /// <summary>
        /// Gets the track number of the format.
        /// </summary>
        public int Track { get { return _streamIndex; } }

        /// <summary>
        /// Gets the index of the format.
        /// </summary>
        public int Index { get { return _typeIndex; } }

        /// <summary>
        /// Gets the width of the video frames of the format, in pixels.
        /// </summary>
        public int VideoWidth { get { return _width; } }

        /// <summary>
        /// Gets the height of the video frames of the format, in pixels.
        /// </summary>
        public int VideoHeight { get { return _height; } }

        /// <summary>
        /// Gets the video frame rate of the format, in frames per second.
        /// </summary>
        public float FrameRate { get { return _frameRate; } }

    }

    #endregion


    // ******************************** Webcam Settings Class

    #region Webcam Settings Class

    /// <summary>
    /// A class that is used to save and restore webcam settings.
    /// </summary>
    [CLSCompliant(true)]
    [Serializable]
    public sealed class WebcamSettings
    {
        internal string         _webcamName;
        internal WebcamFormat   _format;

        internal int    _brightness;
        internal bool   _autoBrightness;

        internal int    _contrast;
        internal bool   _autoContrast;

        internal int    _hue;
        internal bool   _autoHue;

        internal int    _saturation;
        internal bool   _autoSaturation;

        internal int    _sharpness;
        internal bool   _autoSharpness;

        internal int    _gamma;
        internal bool   _autoGamma;

        internal int    _whiteBalance;
        internal bool   _autoWhiteBalance;

        internal int    _gain;
        internal bool   _autoGain;

        internal int    _zoom;
        internal bool   _autoZoom;

        internal int    _focus;
        internal bool   _autoFocus;

        internal int    _exposure;
        internal bool   _autoExposure;

        internal int    _iris;
        internal bool   _autoIris;

        internal int    _pan;
        internal bool   _autoPan;

        internal int    _tilt;
        internal bool   _autoTilt;

        internal int    _roll;
        internal bool   _autoRoll;

        internal int    _flash;
        internal bool   _autoFlash;

        internal int    _backlight;
        internal bool   _autoBacklight;

        internal int    _colorEnable;
        internal bool   _autoColorEnable;

        internal int    _powerLine;
        internal bool   _autoPowerLine;
    }

    #endregion


    // ******************************** Audio Track Class

    #region Audio Track Class

    /// <summary>
    /// A class that is used to store media audio track information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class AudioTrack : HideObjectMembers
    {
        #region Fields (Audio Track Class)

        internal Guid   _mediaType;
        internal string _name;
        internal string _language;
        internal int    _channelCount;
        internal int    _sampleRate;
        internal int    _bitDepth;
        internal int    _bitRate;

        #endregion

        internal AudioTrack() { }

        /// <summary>
        /// Gets the media type (GUID) of the track (see Media Foundation documentation).
        /// </summary>
        public Guid MediaType { get { return _mediaType; } }

        /// <summary>
        /// Gets the name of the track.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Gets the language of the track.
        /// </summary>
        public string Language { get { return _language; } }

        /// <summary>
        /// Gets the number of channels in the track.
        /// </summary>
        public int ChannelCount { get { return _channelCount; } }

        /// <summary>
        /// Gets the sample rate of the track.
        /// </summary>
        public int SampleRate { get { return _sampleRate; } }

        /// <summary>
        /// Gets the bit depth of the track.
        /// </summary>
        public int BitDepth { get { return _bitDepth; } }

        /// <summary>
        /// Gets the bit rate of the track.
        /// </summary>
        public int BitRate { get { return _bitRate; } }
    }

    #endregion


    // ******************************** Audio Stream Struct

    #region Audio Stream Struct

    internal struct AudioStream
    {
        internal Guid   MediaType;
        internal int    StreamIndex;
        internal bool   Selected;
        internal string Name;
        internal string Language;
        internal int    ChannelCount;
        internal int    ChannelCountRestore;
        internal int    SampleRate;
        internal int    BitDepth;
        internal int    BitRate;
    }

    #endregion


    // ******************************** Audio Device Class

    #region Audio Device Class

    /// <summary>
    /// A class that is used to provide audio output device information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class AudioDevice : DeviceInfo
    {
        internal AudioDevice() { }
    }

    #endregion


    // ******************************** Audio Input Device Class

    #region Audio Input Device Class

    /// <summary>
    /// A class that is used to provide audio input device information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class AudioInputDevice : DeviceInfo
    {
        internal AudioInputDevice() { }
    }

    #endregion


    // ******************************** Slider Value Class

    #region Slider Value Class

    /// <summary>
    /// A static class that provides location information for values on a slider (trackbar).
    /// </summary>
    [CLSCompliant(true)]
    public static class SliderValue
    {
        #region Fields (Slider Value Class))

        // standard .Net TrackBar track margins (pixels between border and begin/end of track)
        private const int SLIDER_LEFT_MARGIN    = 13;
        private const int SLIDER_RIGHT_MARGIN   = 14;
        private const int SLIDER_TOP_MARGIN     = 13;
        private const int SLIDER_BOTTOM_MARGIN  = 14;

        #endregion

        /// <summary>
        /// Returns the slider value at the specified location on the specified slider (trackbar).
        /// </summary>
        /// <param name="slider">The slider whose value should be obtained.</param>
        /// <param name="location">The relative x- and y-coordinates on the slider.</param>
        public static int FromPoint(TrackBar slider, Point location)
        {
            return FromPoint(slider, location.X, location.Y);
        }

        /// <summary>
        /// Returns the slider value at the specified location on the specified slider (trackbar).
        /// </summary>
        /// <param name="slider">The slider whose value should be obtained.</param>
        /// <param name="x">The relative x-coordinate on the slider (for horizontal oriented sliders).</param>
        /// <param name="y">The relative y-coordinate on the slider (for vertical oriented sliders).</param>
        public static int FromPoint(TrackBar slider, int x, int y)
        {
			if (slider == null) return 0;

			float pos;
			if (slider.Orientation == Orientation.Horizontal)
			{
				if (x <= SLIDER_LEFT_MARGIN) pos = 0;
				else if (x >= slider.Width - SLIDER_LEFT_MARGIN) pos = 1;
				else pos = (float)(x - SLIDER_LEFT_MARGIN) / (slider.Width - (SLIDER_LEFT_MARGIN + SLIDER_RIGHT_MARGIN));
			}
			else
			{
				if (y <= SLIDER_TOP_MARGIN) pos = 1;
				else if (y >= slider.Height - SLIDER_TOP_MARGIN) pos = 0;
				else pos = 1 - (float)(y - SLIDER_TOP_MARGIN) / (slider.Height - (SLIDER_TOP_MARGIN + SLIDER_BOTTOM_MARGIN));
			}
			return (int)(pos * (slider.Maximum - slider.Minimum)) + slider.Minimum;
        }

        /// <summary>
        /// Returns the location of the specified value on the specified slider (trackbar).
        /// </summary>
        /// /// <param name="slider">The slider whose value location should be obtained.</param>
        /// <param name="value">The value of the slider.</param>
        public static Point ToPoint(TrackBar slider, int value)
        {
			Point result = Point.Empty;
			if (slider != null)
			{
				double pos = 0;
				if (value > slider.Minimum)
				{
					if (value >= slider.Maximum) pos = 1;
					else pos = (double)(value - slider.Minimum) / (slider.Maximum - slider.Minimum);
				}

				if (slider.Orientation == Orientation.Horizontal) result.X = (int)(pos * (slider.Width - (SLIDER_LEFT_MARGIN + SLIDER_RIGHT_MARGIN)) + 0.5) + SLIDER_LEFT_MARGIN;
				else result.Y = (int)(pos * (slider.Height - (SLIDER_TOP_MARGIN + SLIDER_BOTTOM_MARGIN)) + 0.5) + SLIDER_TOP_MARGIN;
			}
			return result;
		}
    }

    #endregion


    // ******************************** Metadata Class

    #region Metadata Class

    /// <summary>
    /// A class that is used to store metadata properties obtained from media files.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class Metadata : HideObjectMembers, IDisposable
    {
        #region Fields (Metadata Class)

        internal string     _artist;
        internal string     _albumArtist;
        internal string     _title;
        internal string     _subTitle;
        internal string     _album;
        internal int        _trackNumber;
        internal string     _year;
        internal TimeSpan   _duration;
        internal string     _genre;
        internal string     _comment;
        internal Image      _image;

        private bool        _disposed;

        #endregion

        internal Metadata() { }

        /// <summary>
        /// Gets the artist(s)/performer(s)/band/orchestra of the media.
        /// </summary>
        public string Artist { get { return _artist; } }

        /// <summary>
        /// Gets the main artist(s)/performer(s)/band/orchestra of the media.
        /// </summary>
        public string AlbumArtist { get { return _albumArtist; } }

        /// <summary>
        /// Gets the title of the media.
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// Gets the subtitle of the media.
        /// </summary>
        public string SubTitle { get { return _subTitle; } }

        /// <summary>
        /// Gets the title of the album that contains the media.
        /// </summary>
        public string AlbumTitle { get { return _album; } }

        /// <summary>
        /// Gets the track number of the media.
        /// </summary>
        public int TrackNumber { get { return _trackNumber; } }

        /// <summary>
        /// Gets the year the media was published.
        /// </summary>
        public string Year { get { return _year; } }

        /// <summary>
        /// Gets the duration (length) of the media.
        /// </summary>
        public TimeSpan Duration { get { return _duration; } }

        /// <summary>
        /// Gets the genre of the media.
        /// </summary>
        public string Genre { get { return _genre; } }

        /// <summary>
        /// Gets the comment attached to the media.
        /// </summary>
        public string Comment { get { return _comment; } }

        /// <summary>
        /// Gets the image attached to the media.
        /// </summary>
        public Image Image { get { return _image; } }

        /// <summary>
        /// Remove the metadata information and clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed   = true;
                _artist     = null;
                _title      = null;
                _album      = null;
                _year       = null;
                _genre      = null;
                if (_image != null)
                {
                    try { _image.Dispose(); }
                    catch { /* ignored */ }
                    _image = null;
                }
            }
        }
    }

    #endregion


    // ******************************** Media Chapter Class

    #region Media Chapter Class

    /// <summary>
    /// A class that is used to store media chapter information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class MediaChapter : HideObjectMembers
    {
        #region Fields (Media Chapter Class)

        internal string[]   _title;
        internal string[]   _language;
        internal TimeSpan   _startTime;
        internal TimeSpan   _endTime;

        #endregion

        internal MediaChapter() { }

        /// <summary>
        /// Initializes a new instance of the MediaChapter class.
        /// </summary>
        /// <param name="title">The title of the media chapter.</param>
        /// <param name="startTime">The start time of the media chapter.</param>
        /// <param name="endTime">The end time of the media chapter. TimeSpan.Zero indicates the beginning of the next chapter or the end of the media.</param>
        public MediaChapter(string title, TimeSpan startTime, TimeSpan endTime)
        {
            _title          = new string[1];
            _title[0]       = string.IsNullOrWhiteSpace(title) ? string.Empty : title;
            _startTime      = startTime;
            _endTime        = endTime;
            _language       = new string[1];
            _language[0]    = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the MediaChapter class.
        /// </summary>
        /// <param name="titles">The title(s) of the media chapter.</param>
        /// <param name="startTime">The start time of the media chapter.</param>
        /// <param name="endTime">The end time of the media chapter. TimeSpan.Zero indicates the beginning of the next chapter or the end of the media.</param>
        /// <param name="languages">The language(s) (3 letter name (ISO 639.2)) of the title(s) of the media chapter. Must be the same number as the number of titles.</param>
        public MediaChapter(string[] titles, TimeSpan startTime, TimeSpan endTime, string[] languages)
        {
            if (titles == null)
            {
                _title          = new string[1];
                _title[0]       = string.Empty;
                _language       = new string[1];
                _language[0]    = string.Empty;
            }
            else
            {
                //_title = new string[titles.Length];
                //titles.CopyTo(_title, 0);
                _title = titles;

                if (languages == null)
                {
                    _language    = new string[1];
                    _language[0] = string.Empty;
                }
                else
                {
                    //_language = new string[languages.Length];
                    //languages.CopyTo(_language, 0);
                    _language = languages;
                }
            }
            _startTime  = startTime;
            _endTime    = endTime;
        }

        /// <summary>
        /// Gets the title of the media chapter. The chapter can have multiple titles in different languages when extracted from a media file.
        /// </summary>
        public string[] Title
        {
            get { return _title; }
        }

        /// <summary>
        /// Gets the language (3 letter name (ISO 639.2)) used for the title of the media chapter or null if not available. The index of the language corresponds to the index of the title.
        /// </summary>
        public string[] Language
        {
            get { return _language; }
        }

        /// <summary>
        /// Gets the start time of the media chapter.
        /// </summary>
        public TimeSpan StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// Gets the end time of the media chapter. TimeSpan.Zero indicates the beginning of the next chapter or the end of the file.
        /// </summary>
        public TimeSpan EndTime
        {
            get { return _endTime; }
        }
    }

    #endregion


    // ******************************** Recording Class

    #region Recording Class

    /// <summary>
    /// A class that is used to store recording information.
    /// </summary>
    [CLSCompliant(true)]
    public sealed class Recording : HideObjectMembers
    {
        #region Fields (Recording Class)

        internal string     _recorderName;
        internal string     _fileName;

        internal bool       _hasAudio;
        internal string     _audioDeviceName;
        internal AudioTrack _audio;

        internal bool       _hasVideo;
        internal string     _videoDeviceName;
        internal VideoTrack _video;

        #endregion

        internal Recording() { }

        /// <summary>
        /// Gets the  name of the recorder (player) used for the recording.
        /// </summary>
        public string RecorderName { get { return _recorderName; } }

        /// <summary>
        /// Gets the path and file name of the recording.
        /// </summary>
        public string FileName { get { return _fileName; } }

        /// <summary>
        /// Gets a value indicating whether the recording contains audio.
        /// </summary>
        public bool HasAudio { get { return _hasAudio; } }

        /// <summary>
        /// Gets the name of the audio device used for the recording.
        /// </summary>
        public string AudioDeviceName { get { return _audioDeviceName; } }

        /// <summary>
        /// Gets audio information of the recording, for example Audio.SampleRate.
        /// </summary>
        public AudioTrack Audio { get { return _audio; } }

        /// <summary>
        /// Gets a value indicating whether the recording contains video.
        /// </summary>
        public bool HasVideo { get { return _hasVideo; } }

        /// <summary>
        /// Gets the name of the video device used for the recording.
        /// </summary>
        public string VideoDeviceName { get { return _videoDeviceName; } }

        /// <summary>
        /// Gets video information of the recording, for example Video.FrameRate.
        /// </summary>
        public VideoTrack Video { get { return _video; } }
    }
    #endregion


    // ******************************** Display Clone Properties Class

    #region Display Clone Properties Class

        /// <summary>
        /// A class that is used to store display clone properties.
        /// </summary>
        [CLSCompliant(true)]
    public sealed class CloneProperties : HideObjectMembers
    {
        #region Fields (Clone Properties Class)

        internal CloneQuality   _quality        = CloneQuality.Auto;
        internal CloneLayout    _layout         = CloneLayout.Zoom;
        internal CloneFlip      _flip           = CloneFlip.FlipNone;
        internal DisplayShape   _shape          = DisplayShape.Normal;
        internal bool           _videoShape     = true;
        internal bool           _dragEnabled;
        internal Cursor         _dragCursor     = Cursors.SizeAll;

        #endregion

        /// <summary>
        /// Gets or sets the video quality of the display clone (default: CloneQuality.Auto).
        /// </summary>
        public CloneQuality Quality
        {
            get { return _quality; }
            set { _quality = value; }
        }

        /// <summary>
        /// Gets or sets the video layout (display mode) of the display clone (default: CloneLayout.Zoom).
        /// </summary>
        public CloneLayout Layout
        {
            get { return _layout; }
            set { _layout = value; }
        }

        /// <summary>
        /// Gets or sets the video flip mode of the display clone (default: CloneFlip.FlipNone).
        /// </summary>
        public CloneFlip Flip
        {
            get { return _flip; }
            set { _flip = value; }
        }

        /// <summary>
        /// Gets or sets the shape of the display clone window (default: DisplayShape.Normal). See also: CloneProperties.ShapeVideo.
        /// </summary>
        public DisplayShape Shape
        {
            get { return _shape; }
            set { _shape = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the CloneProperties.Shape property applies to the video image (or to the display window) of the display clone (default: true (video)).
        /// </summary>
        public bool ShapeVideo
        {
            get { return _videoShape; }
            set { _videoShape = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parent window (form) of the display clone can be moved by dragging the display clone window (default: false). See also: CloneProperties.DragCursor.
        /// </summary>
        public bool DragEnabled
        {
            get { return _dragEnabled; }
            set { _dragEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the cursor that is used when the display clone window is dragged (default: Cursors.SizeAll). See also: CloneProperties.DragEnabled.
        /// </summary>
        public Cursor DragCursor
        {
            get { return _dragCursor; }
            set { _dragCursor = value; }
        }
    }

	#endregion


	// ******************************** OverlayForm Class

	#region OverlayForm Class

	/// <summary>
	/// A class that can be used as a base class for ABX.MediaPlayer display overlays of type Form. Contains logic to prevent unwanted activation of the overlay.
	/// </summary>
	[CLSCompliant(true)]
    public class OverlayForm : Form
    {
        private bool _clickThrough;

        /// <summary>
        /// Gets or sets a value that indicates whether the overlay is transparent for mouse events (default: false).
        /// </summary>
        public bool ClickThrough
        {
            get { return _clickThrough; }
            set { _clickThrough = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the window will be activated when it is shown (overlay override: not activated).
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
		/// Raises the Control.HandleCreated event.
		/// </summary>
		/// <param name="e">An EventArgs that contains the event data.</param>
		protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // switch off form animation
            if (Environment.OSVersion.Version.Major >= 6)
            {
#pragma warning disable CA1806 // Do not ignore method results
                SafeNativeMethods.DwmSetWindowAttribute(Handle, SafeNativeMethods.DWMWA_TRANSITIONS_FORCEDISABLED, true, 4);
#pragma warning restore CA1806 // Do not ignore method results
            }
        }

        /// <summary>
        /// Processes Windows messages.
        /// </summary>
        /// <param name="m">The Windows Message to process.</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = -1;
            const int WM_MOUSEACTIVATE = 0x0021;
            const int MA_NOACTIVATE = 0x0003;

            // prevent form activation
            if (m.Msg == WM_NCHITTEST)
            {
                if (_clickThrough) m.Result = (IntPtr)HTTRANSPARENT;   // this is for "click through":
                else base.WndProc(ref m);
            }
            else if (m.Msg == WM_MOUSEACTIVATE) m.Result = (IntPtr)MA_NOACTIVATE;   // this is for "don't activate with mouse click":
            else base.WndProc(ref m);
        }
    }

    #endregion


    // ******************************** OverlayLabel Class

    #region OverlayLabel Class

    /// <summary>
    /// A class that can be used as a base class for labels on ABX.MediaPlayer display overlays. Contains logic for better display of text on transparent overlays. 
    /// </summary>
    [CLSCompliant(true)]
    public class OverlayLabel : Label
    {
        /// <summary>
        /// Initializes a new instance of the OverlayLabel class.
        /// </summary>
        public OverlayLabel()
        {
            UseCompatibleTextRendering = true;
        }

        /// <summary>
        /// Raises the Control.Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            base.OnPaint(e);
        }
    }

    #endregion


    // ******************************** Player MF CallBack Class

    #region Player MF CallBack Class

    // Media Foundation Player CallBack Class
    internal sealed class MF_PlayerCallBack : IMFAsyncCallback
    {
        private Player              _base;
        private delegate void       EndOfMediaDelegate();
        private EndOfMediaDelegate  CallEndOfMedia;

        public MF_PlayerCallBack(Player player)
        {
            _base           = player;
            CallEndOfMedia  = new EndOfMediaDelegate(_base.AV_EndOfMedia);
        }

        public void Dispose()
        {
            _base           = null;
            CallEndOfMedia  = null;
        }

        public HResult GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue)
        {
            pdwFlags = MFASync.FastIOProcessingCallback;
            pdwQueue = MFAsyncCallbackQueue.Standard;
            return HResult.S_OK;
        }

        public HResult Invoke(IMFAsyncResult result)
        {
            IMFMediaEvent   mediaEvent      = null;
            MediaEventType  mediaEventType  = MediaEventType.MEUnknown;
            bool getNext = true;

            result.GetState(out object stateObj);
            IMFMediaSession session = stateObj as IMFMediaSession;
            if (session == null) session = _base.mf_MediaSession;
            if (session == null) return HResult.MF_E_NOT_AVAILABLE;

            try
            {
                session.EndGetEvent(result, out mediaEvent);
                mediaEvent.GetType(out mediaEventType);
                mediaEvent.GetStatus(out HResult errorCode);

                if (_base._playing)
                {
                    if (mediaEventType == MediaEventType.MEError
                        || (_base._webcamMode && mediaEventType == MediaEventType.MEVideoCaptureDeviceRemoved)
                        || (_base._micMode && mediaEventType == MediaEventType.MECaptureAudioSessionDeviceRemoved))
                    {
                        _base._lastError    = errorCode;
                        errorCode           = Player.NO_ERROR;
                        getNext             = false;
                    }

                    if (errorCode >= 0)
                    {
                        if (!getNext || mediaEventType == MediaEventType.MESessionEnded)
                        {
                            if (getNext)
                            {
                                _base._lastError = Player.NO_ERROR;
                                if (!_base._repeat && !_base._chapterMode) getNext = false;
                            }

                            Control control = _base._display;
                            if (control == null)
                            {
                                FormCollection forms = Application.OpenForms;
                                if (forms != null && forms.Count > 0) control = forms[0];
                            }
                            if (control != null) control.BeginInvoke(CallEndOfMedia);
                            else _base.AV_EndOfMedia();
                        }
                    }
                    else _base._lastError = errorCode;
                }
                else _base._lastError = errorCode;
            }
            finally
            {
                if (getNext && mediaEventType != MediaEventType.MESessionClosed) session.BeginGetEvent(this, session);
                if (mediaEvent != null) Marshal.ReleaseComObject(mediaEvent);

                if (_base.mf_AwaitCallBack)
                {
                    _base.mf_AwaitCallBack = false;
                    _base.WaitForEvent.Set();
                }
                _base.mf_AwaitDoEvents = false;

                if (mediaEventType == MediaEventType.MESessionClosed)
                {
                    _base.AV_ProcessSessionClosed(session);
                }
            }
            return 0;
        }
    }

	#endregion


	// ******************************** Hide System Object Members Classes

	#region  Hide System Object Members Classes

	/// <summary>
	/// Internal class that is used to hide System.Object members in derived classes.
	/// </summary>
	[CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class HideObjectMembers
    {
        #region Hide Inherited System.Object members

        /// <summary>
        /// Gets the type of the current instance.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() { return base.GetType(); } // this can't be hidden ???

        /// <summary>
        /// Serves as a hash function for a particular object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() { return base.GetHashCode(); }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() { return base.ToString(); }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) { return base.Equals(obj); }

        #endregion
    }

    /// <summary>
    /// Internal class that is used to hide System.Object members in the from EventArgs derived classes.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class HideObjectEventArgs : EventArgs
    {
        #region Hide Inherited System.Object members

        /// <summary>
        /// Gets the type of the current instance.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() { return base.GetType(); } // this can't be hidden ???

        /// <summary>
        /// Serves as a hash function for a particular object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() { return base.GetHashCode(); }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() { return base.ToString(); }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) { return base.Equals(obj); }

        #endregion
    }

    #endregion



    // ******************************** Player Grouping Classes

    #region Audio Class

    /// <summary>
    /// A class that is used to group together the Audio methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
	public sealed class Audio : HideObjectMembers
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
	{
        #region Fields (Audio Class)

        #region Constants

        private const int   NO_ERROR                = 0;

		private const int   VOLUME_FADE_INTERVAL    = 20;
        private const int   BALANCE_FADE_INTERVAL   = 10;

        private const int   FADE_INTERVAL_MINIMUM   = 1;
        private const int   FADE_INTERVAL_MAXIMUM   = 100;

        private const float FADE_STEP_VALUE         = 0.01f;

        #endregion


        private Player      _base;

        // Volume fading
        private bool        _volumeIsFading;
        private Timer       _volumeFadeTimer;
        private int         _volumeFadeInterval         = VOLUME_FADE_INTERVAL;
        private float       _volumeFadeStep;
        private float       _volumeFadeEndValue;
        private float       _volumeFadeCurrentValue;

        // Balance fading
        private bool        _balanceIsFading;
        private Timer       _balanceFadeTimer;
        private int         _balanceFadeInterval        = BALANCE_FADE_INTERVAL;
        private float       _balanceFadeStep;
        private float       _balanceFadeEndValue;
        private float       _balanceFadeCurrentValue;

        // Device volume fading
        private bool        _deviceIsFading;
        private Timer       _deviceFadeTimer;
        private int         _deviceFadeInterval         = VOLUME_FADE_INTERVAL;
        private float       _deviceFadeStep;
        private float       _deviceFadeEndValue;
        private float       _deviceFadeCurrentValue;

        #endregion

        internal Audio(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains audio.
        /// </summary>
        public bool Present
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasAudio;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the audio output of the player is enabled (default: true).
        /// </summary>
        public bool Enabled
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioEnabled;
            }
            set { _base.AV_SetAudioEnabled(value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the audio output from the player is muted (default: false).
        /// </summary>
        public bool Mute
        {
            get
            {
                _base._lastError = NO_ERROR;
                return !_base._audioEnabled;
            }
            set { _base.AV_SetAudioEnabled(!value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether audio tracks in subsequent media files are ignored by the player (default: false). The audio track information remains available. Allows to play video from media with unsupported audio formats.
        /// </summary>
        public bool Cut
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioCut;
            }
            set
            {
                _base._audioCut = value;
                if (value) _base._videoCut = false;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the audio output from the player is in mono mode (default: false).
        /// </summary>
        public bool Mono
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._audioMono;
			}
			set
			{
				if (value != _base._audioMono)
				{
					_base._audioMono = value;
                    if (_base._playing && _base._hasAudio)
                    {
                        _base._audioMonoRestore = !value;
                        _base.AV_UpdateTopology();
                    }
				}
				_base._lastError = NO_ERROR;
			}
		}

		// Tracks:

		/// <summary>
		/// Gets or sets the active audio track of the playing media. See also: Player.Audio.TrackCount and Player.Audio.GetTracks.
		/// </summary>
		public int Track
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioTrackCurrent;
            }
            set { _base.AV_SetTrack(value, true); }
        }

        /// <summary>
        /// Gets the number of audio tracks in the playing media. See also: Player.Audio.Track and Player.Audio.GetTracks.
        /// </summary>
        public int TrackCount
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioTrackCount;
            }
        }

        /// <summary>
        /// Returns the audio tracks in the playing media or null if none are present. See also: Player.Audio.Track and Player.Audio.TrackCount.
        /// </summary>
        public AudioTrack[] GetTracks()
        {
            return _base.AV_GetAudioTracks();
        }

        // Channels:

        /// <summary>
        /// Gets the number of audio channels (e.g. 2 for stereo) in the active audio track of the playing media. See also: Player.Audio.DeviceChannelCount.
        /// </summary>
        public int ChannelCount
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (!_base._playing) return 0;
                return _base._mediaChannelCount;
            }
        }

        /// <summary>
        /// Gets or sets the volume of each individual audio output channel (up to 16 channels) of the player, values from 0.0 (mute) to 1.0 (max). See also: Player.Audio.ChannelCount and Player.Audio.DeviceChannelCount.
        /// </summary>
        public float[] ChannelVolumes
        {
            get
            {
                _base._lastError = NO_ERROR;

                float[] volumes = new float[Player.MAX_AUDIO_CHANNELS];
                for (int i = 0; i < Player.MAX_AUDIO_CHANNELS; i++)
                {
                    volumes[i] = _base._audioChannelsVolume[i];
                }
                return volumes;
            }
            set
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                if (value != null && value.Length > 0)
                {
                    if (_base._audioChannelsVolumeCopy == null) _base._audioChannelsVolumeCopy = new float[Player.MAX_AUDIO_CHANNELS];

                    int length = value.Length;
                    bool valid = true;
                    float newVolume = 0.0f;

                    for (int i = 0; i < Player.MAX_AUDIO_CHANNELS; i++)
                    {
                        if (i < length)
                        {
                            if (value[i] < 0.0f || value[i] > 1.0f)
                            {
                                valid = false;
                                break;
                            }
                            _base._audioChannelsVolumeCopy[i] = value[i];
                            if (i < 2 && value[i] > newVolume) newVolume = value[i];
                        }
                        else _base._audioChannelsVolumeCopy[i] = _base._audioChannelsVolume[i]; //  0.0f;
                    }

                    if (valid)
                    {
                        _base._lastError = NO_ERROR;

                        float newBalance;
                        if (value[0] >= value[1])
                        {
                            if (value[0] == 0.0f) newBalance = 0.0f;
                            else newBalance = (value[1] / value[0]) - 1;
                        }
                        else
                        {
                            if (value[1] == 0.0f) newBalance = 0.0f;
                            else newBalance = 1 - (value[0] / value[1]);
                        }

                        _base.AV_SetAudioChannels(_base._audioChannelsVolumeCopy, newVolume, newBalance);

                        //if (!_base._audioEnabled)
                        //{
                        //    _base._audioEnabled = true;
                        //    if (_base._mediaAudioMuteChanged != null) _base._mediaAudioMuteChanged(_base, EventArgs.Empty);
                        //}
                    }
                }
            }
        }

        // Volume:

        /// <summary>
        /// Gets or sets the audio volume of the player, values from 0.0 (mute) to 1.0 (max) (default: 1.0).
        /// </summary>
        public float Volume
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioVolume;
            }
            set { _base.AV_SetAudioVolume(value, true, true); }
        }

        /// <summary>
        /// Gradually increases or decreases the audio volume of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        public int VolumeTo(float level)
        {
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else
            {
                if (_volumeIsFading)
                {
                    _volumeFadeCurrentValue = _base._audioVolume;
                    _volumeFadeEndValue     = level;
                    _volumeFadeStep         = _volumeFadeEndValue > _volumeFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;
                }
                else if (level != _base._audioVolume)
                {
                    _volumeIsFading         = true;
                    _volumeFadeCurrentValue = _base._audioVolume;

                    _volumeFadeEndValue     = level;
                    _volumeFadeStep         = _volumeFadeEndValue > _volumeFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;

					_volumeFadeTimer        = new Timer
					{
						Interval            = _volumeFadeInterval
					};
					_volumeFadeTimer.Tick   += VolumeStep_Tick;
                    _volumeFadeTimer.Start();
                }
                _base._lastError = NO_ERROR;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gradually increases or decreases the audio volume of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        /// <param name="interval">The time, in milliseconds, between two consecutive volume values. Values from 1 to 100 (default: 20). This interval is used until it is changed again.</param>
        public int VolumeTo(float level, int interval)
        {
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM || interval < FADE_INTERVAL_MINIMUM || interval > FADE_INTERVAL_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                return (int)_base._lastError;
            }

            _volumeFadeInterval = interval;
            if (_volumeIsFading) _volumeFadeTimer.Interval = interval;

            return VolumeTo(level);
        }

        private void VolumeStep_Tick(object sender, EventArgs e)
        {
            bool endReached = Math.Abs(_base._audioVolume - _volumeFadeEndValue) < FADE_STEP_VALUE;
            if (endReached || _volumeFadeCurrentValue != _base._audioVolume)
            {
                _volumeIsFading = false;
                _volumeFadeTimer.Dispose();
                _volumeFadeTimer = null;
                if (endReached) _base.AV_SetAudioVolume(_volumeFadeEndValue, true, true);
            }
            else
            {
                _volumeFadeCurrentValue += _volumeFadeStep;
                _base.AV_SetAudioVolume(_volumeFadeCurrentValue, true, true);
                Application.DoEvents();
            }
        }

        // Balance:

        /// <summary>
        /// Gets or sets the audio balance of the player, values from -1.0 (left) to 1.0 (right) (default: 0.0).
        /// </summary>
        public float Balance
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioBalance;
            }
            set { _base.AV_SetAudioBalance(value, true, true); }
        }

        /// <summary>
        /// Gradually changes the audio balance of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio balance level to be set, values from -1.0 (left) to 1.0 (right).</param>
        public int BalanceTo(float level)
        {
            if (level < Player.AUDIO_BALANCE_MINIMUM || level > Player.AUDIO_BALANCE_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else
            {
                if (_balanceIsFading)
                {
                    _balanceFadeCurrentValue    = _base._audioBalance;
                    _balanceFadeEndValue        = level;
                    _balanceFadeStep            = _balanceFadeEndValue > _balanceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;
                }
                else if (level != _base._audioBalance)
                {
                    _balanceIsFading            = true;
                    _balanceFadeCurrentValue    = _base._audioBalance;

                    _balanceFadeEndValue        = level;
                    _balanceFadeStep            = _balanceFadeEndValue > _balanceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;

					_balanceFadeTimer           = new Timer
					{
						Interval                = _balanceFadeInterval
					};
					_balanceFadeTimer.Tick      += BalanceStep_Tick;
                    _balanceFadeTimer.Start();
                }
                _base._lastError = NO_ERROR;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gradually changes the audio balance of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio balance level to be set, values from -1.0 (left) to 1.0 (right).</param>
        /// <param name="interval">The time, in milliseconds, between two consecutive balance values. Values from 1 to 100 (default: 10). This interval is used until it is changed again.</param>
        public int BalanceTo(float level, int interval)
        {
            if (level < Player.AUDIO_BALANCE_MINIMUM || level > Player.AUDIO_BALANCE_MAXIMUM || interval < FADE_INTERVAL_MINIMUM || interval > FADE_INTERVAL_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                return (int)_base._lastError;
            }

            _balanceFadeInterval = interval;
            if (_balanceIsFading) _balanceFadeTimer.Interval = interval;

            return BalanceTo(level);
        }

        private void BalanceStep_Tick(object sender, EventArgs e)
		{
            bool endReached = Math.Abs(_base._audioBalance - _balanceFadeEndValue) < FADE_STEP_VALUE;
            if (endReached || _balanceFadeCurrentValue != _base._audioBalance)
            {
                _balanceIsFading = false;
                _balanceFadeTimer.Dispose();
                _balanceFadeTimer = null;
                if (endReached) _base.AV_SetAudioBalance(_balanceFadeEndValue, true, true);
            }
            else
            {
                _balanceFadeCurrentValue += _balanceFadeStep;
                _base.AV_SetAudioBalance(_balanceFadeCurrentValue, true, true);
                Application.DoEvents();
            }
        }

		
        // Audio Devices:

		/// <summary>
		/// Gets the number of the system's enabled audio output devices. See also: Player.Audio.GetDevices.
		/// </summary>
		public int DeviceCount
        {
            get
            {
                uint count = 0;

                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                deviceEnumerator.EnumAudioEndpoints(EDataFlow.eRender, (uint)DeviceState.Active, out IMMDeviceCollection deviceCollection);
                Marshal.ReleaseComObject(deviceEnumerator);

                if (deviceCollection != null)
                {
                    deviceCollection.GetCount(out count);
                    Marshal.ReleaseComObject(deviceCollection);
                }

                _base._lastError = NO_ERROR;
                return (int)count;
            }
        }

        /// <summary>
        /// Returns the system's enabled audio output devices or null if none are present. See also: Player.Audio.DeviceCount and Player.Audio.GetDefaultDevice.
        /// </summary>
        public AudioDevice[] GetDevices()
        {
            AudioDevice[] audioDevices = null;
            _base._lastError = HResult.MF_E_NO_AUDIO_PLAYBACK_DEVICE;

            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            deviceEnumerator.EnumAudioEndpoints(EDataFlow.eRender, (uint)DeviceState.Active, out IMMDeviceCollection deviceCollection);
            Marshal.ReleaseComObject(deviceEnumerator);

            if (deviceCollection != null)
            {
                deviceCollection.GetCount(out uint count);
                if (count > 0)
                {
                    audioDevices = new AudioDevice[count];
                    for (int i = 0; i < count; i++)
                    {
                        audioDevices[i] = new AudioDevice();

                        deviceCollection.Item((uint)i, out IMMDevice device);
                        Player.GetDeviceInfo(device, audioDevices[i]);

                        Marshal.ReleaseComObject(device);
                    }
                    _base._lastError = NO_ERROR;
                }
                Marshal.ReleaseComObject(deviceCollection);
            }
            return audioDevices;
        }

        /// <summary>
        /// Returns the system's default audio output device or null if not present. See also: Player.Audio.GetDevices.
        /// </summary>
        public AudioDevice GetDefaultDevice()
        {
            AudioDevice audioDevice = null;

            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice device);
            Marshal.ReleaseComObject(deviceEnumerator);

            if (device != null)
            {
                audioDevice = new AudioDevice();
                Player.GetDeviceInfo(device, audioDevice);

                Marshal.ReleaseComObject(device);
                _base._lastError = NO_ERROR;
            }
            else
            {
                _base._lastError = HResult.MF_E_NO_AUDIO_PLAYBACK_DEVICE;
            }

            return audioDevice;
        }


		// Active Audio Device:

		/// <summary>
		/// Gets or sets the audio output device used by the player (default: null). The default audio output device of the system is indicated by null. See also: Player.Audio.GetDevices and Player.Audio.GetDefaultDevice.
		/// </summary>
		public AudioDevice Device
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioDevice;
            }
            set
            {
                _base._lastError = NO_ERROR;
                bool setDevice = false;

                if (value == null)
                {
                    if (_base._audioDevice != null)
                    {
                        _base._audioDevice = null;
                        setDevice = true;
                    }
                }
                else if (_base._audioDevice == null || value._id != _base._audioDevice._id)
                {
                    AudioDevice[] devices = GetDevices();
                    for (int i = 0; i < devices.Length; i++)
                    {
                        if (value._id == devices[i]._id)
                        {
                            _base._audioDevice = devices[i];
                            setDevice = true;
                            break;
                        }
                    }
                    if (!setDevice) _base._lastError = HResult.ERROR_SYSTEM_DEVICE_NOT_FOUND;
                }

                if (setDevice)
                {
                    if (_base._hasAudio) // = also playing
                    {
                        _base.AV_UpdateTopology();
                    }

                    if (_base._lastError == NO_ERROR)
                    {
                        if (_base.pm_HasPeakMeter)
                        {
                            _base.StartSystemDevicesChangedHandlerCheck();
                            _base.PeakMeter_Open(_base._audioDevice, true);
                        }
                        else
                        {
                            if (_base._audioDevice == null) _base.StopSystemDevicesChangedHandlerCheck();
                            else _base.StartSystemDevicesChangedHandlerCheck();
                        }
                        _base._mediaAudioDeviceChanged?.Invoke(_base, EventArgs.Empty);
                    }
                    else
                    {
                        _base.AV_CloseSession(false, true, StopReason.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume of the audio output device of the player, values from 0.0 (mute) to 1.0 (max).
        /// </summary>
        public float DeviceVolume
        {
            get
            {
                float volume = Player.AudioDevice_MasterVolume(_base._audioDevice, 0, false);
                if (volume == -1)
                {
                    volume = 0;
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE; // device not ready
                }
                else _base._lastError = NO_ERROR;

                return volume;
            }
            set
            {
                if (value < Player.AUDIO_VOLUME_MINIMUM || value > Player.AUDIO_VOLUME_MAXIMUM)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    float volume = Player.AudioDevice_MasterVolume(_base._audioDevice, value, true);
                    if (volume == -1) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    else
                    {
                        if (volume < 0.001) DeviceMute = true;
                        else if (DeviceMute) DeviceMute = false;
                        _base._lastError = NO_ERROR;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the muting state of the audio output device of the player.
        /// </summary>
        public bool DeviceMute
        {
            get
            {
                bool muteState = false;
                _base._lastError = (HResult)Player.AudioDevice_MasterMute(_base._audioDevice, ref muteState, false);
                return muteState;

            }
            set
            {
                bool muteState = value;
                _base._lastError = (HResult)Player.AudioDevice_MasterMute(_base._audioDevice, ref muteState, true);
            }
        }

        /// <summary>
        /// Gets the number of audio output channels of the player's audio output device. See also: Player.Audio.ChannelCount.
        /// </summary>
        public int DeviceChannelCount
        {
            get
            {
                _base._lastError = NO_ERROR;

                if (_base.pm_HasPeakMeter) return _base.pm_PeakMeterChannelCount;
                else return Player.Device_GetChannelCount(_base._audioDevice);
            }
        }

        /// <summary>
        /// Gradually increases or decreases the volume of the audio output device of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        public int DeviceVolumeTo(float level)
        {
            float currentVolume = DeviceVolume;
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else if (currentVolume != -1)
            {
                if (_deviceIsFading)
                {
                    _deviceFadeCurrentValue     = currentVolume;
                    _deviceFadeEndValue         = level;
                    _deviceFadeStep             = _deviceFadeEndValue > _deviceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;
                }
                else if (level != DeviceVolume)
                {
                    _deviceIsFading             = true;
                    _deviceFadeCurrentValue     = currentVolume;

                    _deviceFadeEndValue         = level;
                    _deviceFadeStep             = _deviceFadeEndValue > _deviceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;

					_deviceFadeTimer            = new Timer
					{
						Interval                = _deviceFadeInterval
					};
					_deviceFadeTimer.Tick       += DeviceVolumeStep_Tick;
                    _deviceFadeTimer.Start();
                }
                _base._lastError = NO_ERROR;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gradually increases or decreases the volume of the audio output device of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        /// <param name="interval">The time, in milliseconds, between two consecutive volume values. Values from 1 to 100 (default: 20). This interval is used until it is changed again.</param>
        public int DeviceVolumeTo(float level, int interval)
        {
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM || interval < FADE_INTERVAL_MINIMUM || interval > FADE_INTERVAL_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                return (int)_base._lastError;
            }

            _deviceFadeInterval = interval;
            if (_deviceIsFading) _deviceFadeTimer.Interval = interval;

            return DeviceVolumeTo(level);
        }

        private void DeviceVolumeStep_Tick(object sender, EventArgs e)
        {
            try
            {
                float currentVolume = DeviceVolume;
                bool endReached = currentVolume == -1 || Math.Abs(currentVolume - _deviceFadeEndValue) < FADE_STEP_VALUE;
                if (endReached || Math.Abs(currentVolume - _deviceFadeCurrentValue) > FADE_STEP_VALUE) //_masterFadeCurrentValue != MasterVolume)
                {
                    _deviceIsFading = false;
                    _deviceFadeTimer.Dispose();
                    _deviceFadeTimer = null;
                    if (endReached) DeviceVolume = _deviceFadeEndValue;
                }
                else
                {
                    _deviceFadeCurrentValue += _deviceFadeStep;
                    DeviceVolume = _deviceFadeCurrentValue;
                    Application.DoEvents();
                }
            }
            catch
            {
                _deviceIsFading = false;
                _deviceFadeTimer.Dispose();
                _deviceFadeTimer = null;
            }
        }

    }

    #endregion

    #region Audio Input Class

    /// <summary>
    /// A class that is used to group together the Audio Input methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
	public sealed class AudioInput : HideObjectMembers
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
	{
        #region Fields (Audio Input Class)

        #region Constants

        private const int   NO_ERROR                = 0;

        private const int   VOLUME_FADE_INTERVAL    = 20;

        private const int   FADE_INTERVAL_MINIMUM   = 1;
        private const int   FADE_INTERVAL_MAXIMUM   = 100;

        private const float FADE_STEP_VALUE         = 0.01f;

        #endregion

        private Player      _base;

        // Device volume fading
        private bool        _deviceIsFading;
        private Timer       _deviceFadeTimer;
        private int         _deviceFadeInterval     = VOLUME_FADE_INTERVAL;
        private float       _deviceFadeStep;
        private float       _deviceFadeEndValue;
        private float       _deviceFadeCurrentValue;

        #endregion

        internal AudioInput(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets the number of the system's enabled audio input devices. See also: Player.AudioInput.GetDevices.
        /// </summary>
        public int DeviceCount
        {
            get
            {
                uint count = 0;

                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                deviceEnumerator.EnumAudioEndpoints(EDataFlow.eCapture, (uint)DeviceState.Active, out IMMDeviceCollection deviceCollection);
                Marshal.ReleaseComObject(deviceEnumerator);

                if (deviceCollection != null)
                {
                    deviceCollection.GetCount(out count);
                    Marshal.ReleaseComObject(deviceCollection);
                }

                _base._lastError = NO_ERROR;
                return (int)count;
            }
        }

        /// <summary>
        /// Returns the system's enabled audio input devices or null if none are present. See also: Player.AudioInput.DeviceCount and Player.AudioInput.GetDefaultDevice.
        /// </summary>
        public AudioInputDevice[] GetDevices()
        {
            AudioInputDevice[] audioDevices = null;
            _base._lastError = HResult.MF_E_NO_AUDIO_RECORDING_DEVICE;

            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            deviceEnumerator.EnumAudioEndpoints(EDataFlow.eCapture, (uint)DeviceState.Active, out IMMDeviceCollection deviceCollection);
            Marshal.ReleaseComObject(deviceEnumerator);

            if (deviceCollection != null)
            {
                deviceCollection.GetCount(out uint count);
                if (count > 0)
                {
                    audioDevices = new AudioInputDevice[count];
                    for (int i = 0; i < count; i++)
                    {
                        audioDevices[i] = new AudioInputDevice();

                        deviceCollection.Item((uint)i, out IMMDevice device);
                        Player.GetDeviceInfo(device, audioDevices[i]);

                        Marshal.ReleaseComObject(device);
                    }
                    _base._lastError = NO_ERROR;
                }
                Marshal.ReleaseComObject(deviceCollection);
            }
            return audioDevices;
        }

        /// <summary>
        /// Returns the system's default audio input device or null if not present. See also: Player.AudioInput.GetDevices.
        /// </summary>
        public AudioInputDevice GetDefaultDevice()
        {
            AudioInputDevice audioDevice = null;

            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out IMMDevice device);
            Marshal.ReleaseComObject(deviceEnumerator);

            if (device != null)
            {
                audioDevice = new AudioInputDevice();
                Player.GetDeviceInfo(device, audioDevice);

                Marshal.ReleaseComObject(device);
                _base._lastError = NO_ERROR;
            }
            else
            {
                _base._lastError = HResult.MF_E_NO_AUDIO_RECORDING_DEVICE;
            }

            return audioDevice;
        }

        /// <summary>
        /// Gets a value indicating whether an audio input device is playing (by itself or with a webcam device - including paused audio input). Use the Player.Play method to play an audio input device.
        /// </summary>
        public bool Playing
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_base._webcamMode) return _base._webcamAggregated;
                return _base._micMode;
            }
        }

        /// <summary>
        /// Gets or sets (changes (when not recording)) the audio input device being played (by itself or with a webcam device). Use the Player.Play method to play an audio input device. See also: Player.AudioInput.GetDevices.
        /// </summary>
        public AudioInputDevice Device
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._micDevice;
            }
            set
            {
                if (!_base._recording && (_base._webcamMode || _base._micMode))
                {
                    _base._lastError = NO_ERROR;
                    if (!((value == null && _base._micDevice == null) || (value != null && _base._micDevice != null && value._id == _base._micDevice._id)))
                    {
                        if (_base.pm_HasInputMeter)
                        {
                            _base.InputMeter_Close();
                            _base.pm_InputMeterPending = true;
                        }

                        _base._micDevice = value;
                        _base.AV_UpdateTopology();

                        _base._mediaAudioInputDeviceChanged?.Invoke(_base, EventArgs.Empty);
                    }
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
        }

        /// <summary>
        /// Updates or restores the playing audio input device.
        /// </summary>
        public int Update()
        {
            if (_base._micMode)
            {
                _base._lastError = NO_ERROR;
                _base.AV_UpdateTopology();
            }
            else _base._lastError = HResult.MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets the input volume of the audio input device of the player, values from 0.0 (off) to 1.0 (max).
        /// </summary>
        public float Volume
        {
            get
            {
                float volume = Player.AudioDevice_InputLevel(_base._micDevice, 0, false);
                if (volume == -1)
                {
                    volume = 0;
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE; // device not ready
                }
                else _base._lastError = NO_ERROR;

                return volume;
            }
            set
            {
                if (value < Player.AUDIO_VOLUME_MINIMUM || value > Player.AUDIO_VOLUME_MAXIMUM)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    float volume = Player.AudioDevice_InputLevel(_base._micDevice, value, true);
                    if (volume == -1) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    else _base._lastError = NO_ERROR;
                }
            }
        }

        /// <summary>
        /// Gets or sets the muting state of the audio input device of the player.
        /// </summary>
        public bool Mute
        {
            get
            {
                bool muteState = false;
                _base._lastError = (HResult)Player.AudioDevice_InputMute(_base._micDevice, ref muteState, false);
                return muteState;

            }
            set
            {
                bool muteState = value;
                _base._lastError = (HResult)Player.AudioDevice_InputMute(_base._micDevice, ref muteState, true);
            }
        }

        /// <summary>
        /// Gets the number of audio channels of the player's audio input device.
        /// </summary>
        public int ChannelCount
        {
            get
            {
                _base._lastError = NO_ERROR;

                if (_base.pm_HasInputMeter) return _base.pm_InputMeterChannelCount;
                else if (_base._micDevice != null)  return Player.Device_GetChannelCount(_base._micDevice);
                return 0;
            }
        }

        /// <summary>
        /// Gradually increases or decreases the volume of the audio input device of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio input volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        public int VolumeTo(float level)
        {
            float currentVolume = Volume;
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else if (currentVolume != -1)
            {
                if (_deviceIsFading)
                {
                    _deviceFadeCurrentValue = currentVolume;
                    _deviceFadeEndValue = level;
                    _deviceFadeStep = _deviceFadeEndValue > _deviceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;
                }
                else if (level != Volume)
                {
                    _deviceIsFading = true;
                    _deviceFadeCurrentValue = currentVolume;

                    _deviceFadeEndValue = level;
                    _deviceFadeStep = _deviceFadeEndValue > _deviceFadeCurrentValue ? FADE_STEP_VALUE : -FADE_STEP_VALUE;

                    _deviceFadeTimer = new Timer
                    {
                        Interval = _deviceFadeInterval
                    };
                    _deviceFadeTimer.Tick += VolumeStep_Tick;
                    _deviceFadeTimer.Start();
                }
                _base._lastError = NO_ERROR;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gradually increases or decreases the volume of the audio input device of the player to the specified level.
        /// </summary>
        /// <param name="level">The audio input volume level to be set, values from 0.0 (mute) to 1.0 (max).</param>
        /// <param name="interval">The time, in milliseconds, between two consecutive volume values. Values from 1 to 100 (default: 20). This interval is used until it is changed again.</param>
        public int VolumeTo(float level, int interval)
        {
            if (level < Player.AUDIO_VOLUME_MINIMUM || level > Player.AUDIO_VOLUME_MAXIMUM || interval < FADE_INTERVAL_MINIMUM || interval > FADE_INTERVAL_MAXIMUM)
            {
                _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                return (int)_base._lastError;
            }

            _deviceFadeInterval = interval;
            if (_deviceIsFading) _deviceFadeTimer.Interval = interval;

            return VolumeTo(level);
        }

        private void VolumeStep_Tick(object sender, EventArgs e)
        {
            try
            {
                float currentVolume = Volume;
                bool endReached = currentVolume == -1 || Math.Abs(currentVolume - _deviceFadeEndValue) < FADE_STEP_VALUE;
                if (endReached || Math.Abs(currentVolume - _deviceFadeCurrentValue) > FADE_STEP_VALUE) //_masterFadeCurrentValue != MasterVolume)
                {
                    _deviceIsFading = false;
                    _deviceFadeTimer.Dispose();
                    _deviceFadeTimer = null;
                    if (endReached) Volume = _deviceFadeEndValue;
                }
                else
                {
                    _deviceFadeCurrentValue += _deviceFadeStep;
                    Volume = _deviceFadeCurrentValue;
                    Application.DoEvents();
                }
            }
            catch
            {
                _deviceIsFading = false;
                _deviceFadeTimer.Dispose();
                _deviceFadeTimer = null;
            }
        }
    }

    #endregion

    #region Video Class

    /// <summary>
    /// A class that is used to group together the Video methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Video : HideObjectMembers
    {
        #region Fields (Video Class)

        private const int   NO_ERROR = 0;

        private Player      _base;
        private bool        _zoomBusy;
        private bool        _boundsBusy;
        private Size        _maxZoomSize;

        private VideoOverlay _videoOverlayClass;

        #endregion

        internal Video(Player player)
        {
            _base = player;
            _maxZoomSize = new Size(Player.DEFAULT_VIDEO_WIDTH_MAXIMUM, Player.DEFAULT_VIDEO_HEIGHT_MAXIMUM);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the DirectX Video Acceleration (DXVA) option in the player's topology loader is enabled (default: true).
        /// Disabling this option may (or may not) resolve black screen issues with display clones and screenshots (applies to next media played).
        /// </summary>
        public bool Acceleration
        {
            // MFTOPOLOGY_DXVA_MODE.None == 1
            // MFTOPOLOGY_DXVA_MODE.Full == 2
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoAcceleration == 2;
            }
            set
            {
                _base._lastError = NO_ERROR;
                _base._videoAcceleration = value ? 2 : 1;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains video.
        /// </summary>
        public bool Present
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo;
            }
        }

        /// <summary>
        /// Gets or sets the maximum allowed zoom size (width and height in pixels) of the video image on the player's display window (default: 12000 x 12000).
        /// </summary>
        public Size MaxZoomSize
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _maxZoomSize;
            }
            set
            {
                if (value.Width < Player.VIDEO_WIDTH_MINIMUM || value.Width > Player.VIDEO_WIDTH_MAXIMUM || value.Height < Player.VIDEO_HEIGHT_MINIMUM || value.Height > Player.VIDEO_HEIGHT_MAXIMUM)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    _base._lastError = NO_ERROR;
                    _maxZoomSize = value;
                }
            }
        }

        ///// <summary>
        ///// Gets or sets the maximum allowed width (in pixels) of the video image on the player's display window (default: 6400).
        ///// </summary>
        //public int MaxZoomWidth
        //{
        //    get
        //    {
        //        _base._lastError = NO_ERROR;
        //        return _maxZoomWidth;
        //    }
        //    set
        //    {
        //        if (value < Player.DEFAULT_VIDEO_WIDTH_MAXIMUM || value > Player.VIDEO_WIDTH_MAXIMUM)
        //        {
        //            _base._lastError = HResult.MF_E_OUT_OF_RANGE;
        //        }
        //        else
        //        {
        //            _base._lastError = NO_ERROR;
        //            _maxZoomWidth    = value;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the maximum allowed height (in pixels) of the video image on the player's display window (default: 6400).
        ///// </summary>
        //public int MaxZoomHeight
        //{
        //    get
        //    {
        //        _base._lastError = NO_ERROR;
        //        return _maxZoomHeight;
        //    }
        //    set
        //    {
        //        if (value < Player.DEFAULT_VIDEO_HEIGHT_MAXIMUM || value > Player.VIDEO_HEIGHT_MAXIMUM)
        //        {
        //            _base._lastError = HResult.MF_E_OUT_OF_RANGE;
        //        }
        //        else
        //        {
        //            _base._lastError = NO_ERROR;
        //            _maxZoomHeight   = value;
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets a value that indicates whether the player's full screen display mode on all screens (video wall) is activated (default: false). See also: Player.FullScreenMode.
        /// </summary>
        public bool Wall
        {
            get { return _base.FS_GetVideoWallMode(); }
            set { _base.FS_SetVideoWallMode(value); }
        }

        /// <summary>
        /// Gets or sets the active video track of the playing media. See also: Player.Video.TrackCount and Player.Video.GetTracks.
        /// </summary>
        public int Track
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoTrackCurrent;
            }
            set { _base.AV_SetTrack(value, false); }
        }

        /// <summary>
        /// Gets the number of video tracks in the playing media. See also: Player.Video.Track and Player.Video.GetTracks.
        /// </summary>
        public int TrackCount
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoTrackCount;
            }
        }

        /// <summary>
        /// Returns the video tracks in the playing media or null if none are present. See also: Player.Video.Track and Player.Video.TrackCount.
        /// </summary>
        public VideoTrack[] GetTracks()
        {
            return _base.AV_GetVideoTracks();
        }

        /// <summary>
        /// Gets the original size (width and height) of the video image of the playing media, adjusted for any non-square pixel aspect ratio and rotation, in pixels.
        /// </summary>
        public Size SourceSize
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo ? _base._videoSourceSize : Size.Empty;
            }
        }

        /// <summary>
        /// Gets the video frame rate of the playing media, in frames per second.
        /// </summary>
        public float FrameRate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo ? _base._videoFrameRate : 0;
            }
        }

        /// <summary>
        /// Gets or sets the size and location (in pixels) of the video image on the player's display window. When set, the player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (!_base._hasVideoBounds && _base._hasVideo)
                {
                    _base.AV_GetDisplayModeSize(_base._displayMode);
                }
                return _base._videoBounds;
            }
            set
            {
                if (_base._hasVideo)
                {
                    if (!_boundsBusy)
                    {
                        _boundsBusy = true;

                        if ((value.Width >= Player.VIDEO_WIDTH_MINIMUM) && (value.Width <= _maxZoomSize.Width)
                            && (value.Height >= Player.VIDEO_HEIGHT_MINIMUM) && (value.Height <= _maxZoomSize.Height))
                        {
                            _base._lastError = NO_ERROR;

                            _base._videoBounds = value;
                            _base._videoBoundsClip = Rectangle.Intersect(_base._display.DisplayRectangle, _base._videoBounds);
                            _base._hasVideoBounds = true;

							if (_base._displayMode == DisplayMode.Manual) _base._display.Refresh();
							else _base.Display.Mode = DisplayMode.Manual;

							// TODO - image gets stuck when same size as display - is it _videoDisplay or MF
							if (_base._videoBounds.X <= 0 || _base._videoBounds.Y <= 0)
							{
								_base._videoDisplay.Width--;
								_base._videoDisplay.Width++;
                            }

							if (_base._hasDisplayShape) _base.AV_UpdateDisplayShape();

                            _base._mediaVideoBoundsChanged?.Invoke(_base, EventArgs.Empty);
                        }
                        else _base._lastError = HResult.MF_E_OUT_OF_RANGE;

                        _boundsBusy = false;
                    }
                    else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                }
                else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            }
        }

        // Video Zoom, Move, Stretch

        /// <summary>
        /// Enlarges or reduces the size of the video image at the center location of the player's display window. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        public int Zoom(double factor)
        {
            if (_base._hasVideo) return Zoom(factor, _base._display.Width / 2, _base._display.Height / 2);
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image at the specified display location. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        /// <param name="center">The center location of the zoom on the player's display window.</param>
        public int Zoom(double factor, Point center)
        {
            return (Zoom(factor, center.X, center.Y));
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image at the specified display location. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        /// <param name="xCenter">The horizontal (x) center location of the zoom on the player's display window.</param>
        /// <param name="yCenter">The vertical (y) center location of the zoom on the player's display window.</param>
        public int Zoom(double factor, int xCenter, int yCenter)
        {
            if (_base._hasVideo && factor > 0)
            {
                if (!_zoomBusy)
                {
                    _zoomBusy = true;
                    _base._lastError = NO_ERROR;

                    if (factor != 1)
                    {
                        double width = 0;
                        double height = 0;
                        Rectangle r = new Rectangle(_base._videoBounds.Location, _base._videoBounds.Size);

                        if (r.Width < r.Height)
                        {
                            width = r.Width * factor;
                            if (width > _maxZoomSize.Width)
                            {
                                factor = (double)_maxZoomSize.Width / r.Width;
                                width = r.Width * factor;
                            }
                            else if ((width / r.Width) * r.Height > _maxZoomSize.Height)
                            {
                                factor = (double)_maxZoomSize.Height / r.Height;
                                width = r.Width * factor;
                            }
                            r.X = (int)Math.Round(-factor * (xCenter - r.X)) + xCenter;

                            if (width >= 10)
                            {
                                r.Y = (int)Math.Round(-(width / r.Width) * (yCenter - r.Y)) + yCenter;
                                height = (width / r.Width) * r.Height;
                            }
                        }
                        else
                        {
                            height = r.Height * factor;
                            if (height > _maxZoomSize.Height)
                            {
                                factor = (double)_maxZoomSize.Height / r.Height;
                                height = r.Height * factor;
                            }
                            else if ((height / r.Height) * r.Width > _maxZoomSize.Width)
                            {
                                factor = (double)_maxZoomSize.Width / r.Width;
                                height = r.Height * factor;
                            }
                            r.Y = (int)Math.Round(-factor * (yCenter - r.Y)) + yCenter;

                            if (height >= 10)
                            {
                                r.X = (int)Math.Round(-(height / r.Height) * (xCenter - r.X)) + xCenter;
                                width = (height / r.Height) * r.Width;
                            }
                        }

                        r.Width = (int)Math.Round(width);
                        r.Height = (int)Math.Round(height);
                        Bounds = r;
                    }
                    _zoomBusy = false;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
            else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges the specified part of the player's display window to the entire display window of the player. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="area">The area of the player's display window to enlarge.</param>
        public int Zoom(Rectangle area)
        {
            if (_base._hasVideo)
            {
                if (_base._videoBounds.Width <= _maxZoomSize.Width && _base._videoBounds.Height <= _maxZoomSize.Height && (area.X >= 0 && area.X <= (_base._display.Width - 8)) && (area.Y >= 0 && area.Y <= (_base._display.Height - 8)) && (area.X + area.Width <= _base._display.Width) && (area.Y + area.Height <= _base._display.Height))
                {
                    double factorX = (double)_base._display.Width / area.Width;
                    double factorY = (double)_base._display.Height / area.Height;

                    if (_base._videoBounds.Width * factorX > _maxZoomSize.Width)
                    {
                        double factorX2 = factorX;
                        factorX = (double)_maxZoomSize.Width / _base._videoBounds.Width;
                        factorY *= (factorX / factorX2);
                    }
                    if (_base._videoBounds.Height * factorY > _maxZoomSize.Height)
                    {
                        double factorY2 = factorY;
                        factorY = (double)_maxZoomSize.Height / _base._videoBounds.Height;
                        factorX *= (factorY / factorY2);
                    }

                    Bounds = new Rectangle(
                            (int)(((_base._videoBounds.X - area.X) * factorX)),
                            (int)(((_base._videoBounds.Y - area.Y) * factorY)),
                            (int)((_base._videoBounds.Width * factorX)),
                            (int)((_base._videoBounds.Height * factorY)));
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Moves the location of the video image on the player's display window by the given amount of pixels. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="horizontal">The amount of pixels to move the video image in the horizontal (x) direction.</param>
        /// <param name="vertical">The amount of pixels to move the video image in the vertical (y) direction.</param>
        public int Move(int horizontal, int vertical)
        {
            if (_base._hasVideo)
            {
                Bounds = new Rectangle(_base._videoBounds.X + horizontal, _base._videoBounds.Y + vertical, _base._videoBounds.Width, _base._videoBounds.Height);
            }
            else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image by the given amount of pixels at the center of the video image. The player's display mode (Player.Display.Mode) is set to DisplayMode.Manual.
        /// </summary>
        /// <param name="horizontal">The amount of pixels to stretch the video image in the horizontal (x) direction.</param>
        /// <param name="vertical">The amount of pixels to stretch the video image in the vertical (y) direction.</param>
        public int Stretch(int horizontal, int vertical)
        {
            if (_base._hasVideo)
            {
                Bounds = new Rectangle(_base._videoBounds.X - (horizontal / 2), _base._videoBounds.Y - (vertical / 2), _base._videoBounds.Width + horizontal, _base._videoBounds.Height + vertical);
            }
            else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            return (int)_base._lastError;
        }

        // Video Colors

        /// <summary>
        /// Gets or sets a value that indicates the brightness of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Brightness
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._brightness;
            }
            set
            {
                _base.AV_SetBrightness(value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the contrast of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Contrast
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._contrast;
            }
            set
            {
                _base.AV_SetContrast(value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the hue of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Hue
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hue;
            }
            set
            {
                _base.AV_SetHue(value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the saturation of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Saturation
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._saturation;
            }
            set
            {
                _base.AV_SetSaturation(value, true);
            }
        }

        // Copy to Image

        /// <summary>
        /// Returns a copy of the player's currently displayed video image (without display overlay). See also: Player.Copy.ToImage.
        /// </summary>
        public Image ToImage()
        {
            return _base.AV_DisplayCopy(true, false);
        }

        /// <summary>
        /// Returns a copy of the player's currently displayed video image (without display overlay) with the specified dimensions. See also: Player.Copy.ToImage.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio.</param>
        public Image ToImage(int size)
        {
            Image theImage = null;
            if (size >= 8)
            {
                Image copy = _base.AV_DisplayCopy(true, false);
                if (copy != null)
                {
                    try
                    {
                        //if (copy.Width > copy.Height) theImage = new Bitmap(copy, size, (size * copy.Height) / copy.Width);
                        //else theImage = new Bitmap(copy, (size * copy.Width) / copy.Height, size);
                        if (copy.Width > copy.Height) theImage = Player.AV_ResizeImage(copy, size, (size * copy.Height) / copy.Width);
                        else theImage = Player.AV_ResizeImage(copy, (size * copy.Width) / copy.Height, size);
                    }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            return theImage;
        }

        /// <summary>
        /// Returns a copy of the player's currently displayed video image (without display overlay) with the specified dimensions. See also: Player.Copy.ToImage.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public Image ToImage(int width, int height)
        {
            Image theImage = null;
            if (width >= 8 && height >= 8)
            {
                Image copy = _base.AV_DisplayCopy(true, false);
                if (copy != null)
                {
                    //try { theImage = new Bitmap(copy, width, height); }
                    try { theImage = Player.AV_ResizeImage(copy, width, height); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            return theImage;
        }

        // Copy to Clipboard

        /// <summary>
        /// Copies the player's currently displayed video image (without display overlay) to the system's clipboard. See also: Player.Copy.ToClipboard.
        /// </summary>
        public int ToClipboard()
        {
            Image copy = _base.AV_DisplayCopy(true, false);
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies the player's currently displayed video image (without display overlay) with the specified dimensions to the system's clipboard. See also: Player.Copy.ToClipboard.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio.</param>
        public int ToClipboard(int size)
        {
            Image copy = ToImage(size);
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies the player's currently displayed video image (without display overlay) with the specified dimensions to the system's clipboard. See also: Player.Copy.ToClipboard.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public int ToClipboard(int width, int height)
        {
            Image copy = ToImage(width, height);
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        // Copy to File

        /// <summary>
        /// Saves a copy of the player's currently displayed video image (without display overlay) to the specified file. See also: Player.Copy.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = _base.AV_DisplayCopy(true, false);
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves a copy of the player's currently displayed video image (without display overlay) with the specified dimensions to the specified file. See also: Player.Copy.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="size">The size of the longest side of the image to save while maintaining the aspect ratio.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int size)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = ToImage(size);
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves a copy of the player's currently displayed video image (without display overlay) with the specified dimensions to the specified file. See also: Player.Copy.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="width">The width of the image to save.</param>
        /// <param name="height">The height of the image to save.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int width, int height)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = ToImage(width, height);
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }


        /// <summary>
        /// Gets or sets a value that indicates whether video tracks in subsequent media files are ignored by the player (default: false). The video track information remains available. Allows to play audio from media with unsupported video formats.
        /// </summary>
        public bool Cut
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoCut;
            }
            set
            {
                _base._videoCut = value;
                if (value) _base._audioCut = false;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether video images will be displayed in HD widescreen (16:9) format (default: false). For use with incorrectly displayed video images. See also: Player.Video.AspectRatio.
        /// </summary>
        public bool Widescreen
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoAspectRatio && (_base._videoAspectSize.Width == 16F && _base._videoAspectSize.Height == 9F);
            }
            set
            {
                _base._lastError = _base.AV_SetVideoAspectRatio(value ? new SizeF(16F, 9F) : SizeF.Empty);
            }
        }

        /// <summary>
        /// Gets or sets a custom aspect ratio of video images (for use with incorrectly displayed video images), for example 16:9 (new SizeF(16.0F, 9.0F)) for widescreen (default: 0:0 (SizeF.Empty - normal ratio)). See also: Player.Video.Widescreen.
        /// </summary>
        public SizeF AspectRatio
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoAspectSize;
            }
            set
            {
                _base._lastError = _base.AV_SetVideoAspectRatio(value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates how stereoscopic side-by-side/over-under 3D video is displayed.
        /// </summary>
        public Video3DView View3D
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._video3DView;
            }
            set
            {
				if (value != _base._video3DView)
				{
					_base._video3DView = value;
					if (_base._hasVideo)
					{
						_base.AV_SetVideo3DView();
					}
					else if (_base._video3DView == Video3DView.NormalImage)
					{
						_base._videoCropRect = null;
						_base._videoAspectSize = Size.Empty;
						_base._videoWidthRatio = 0F;
						_base._videoHeightRatio = 0F;
						if (_base._videoAspectRatio)
						{
							_base._videoAspectRatio = false;
							_base._mediaVideoAspectRatioChanged?.Invoke(_base, EventArgs.Empty);
						}

						if (_base._videoCropMode)
						{
							_base._videoCropMode = false;
							_base._mediaVideoCropChanged?.Invoke(_base, EventArgs.Empty);
						}
					}
					_base._mediaVideoView3DChanged?.Invoke(_base, EventArgs.Empty);
				}
				_base._lastError = NO_ERROR;
			}
        }

        /// <summary>
        /// Gets or sets the source rectangle of the video image. The value is a normalized rectangle: the full video image is represented by {0.0F, 0.0F, 1.0F, 1.0F}. Use RectangleF.Empty to restore normal video images (default: RectangleF.Empty).
        /// </summary>
        public RectangleF Crop
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_base._videoCropMode)
                {
                    return new RectangleF(
                        _base._videoCropRect.left,
                        _base._videoCropRect.right,
                        _base._videoCropRect.top,
                        _base._videoCropRect.bottom
                        );
                }
                return RectangleF.Empty;
            }
            set
            {
                bool doUpdate = false;
                _base._lastError = NO_ERROR;

                if (value.IsEmpty || value == new RectangleF(0, 0, 1, 1))
                {
                    if (_base._videoCropMode)
                    {
                        _base._videoCropMode = false;
                        if (_base._hasVideo) _base._videoCropRect = new MFVideoNormalizedRect(0, 0, 1, 1);
                        else _base._videoCropRect = null;
                        doUpdate = true;
                    }
                }
                else
                {
                    if (value.Left >= 0 && value.Left < value.Width &&
                        value.Right >= 0 && value.Right < value.Height &&
                        value.Width <= 1 && value.Height <= 1)
                    {
                        _base._videoCropRect = new MFVideoNormalizedRect(value.Left, value.Right, value.Width, value.Height);
                        _base._videoCropMode = true;
                        doUpdate = true;
                    }
                    else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }

                if (doUpdate)
                {
                    if (_base._video3DView != Video3DView.NormalImage)
                    {
                        _base._video3DView = Video3DView.NormalImage;
                        _base._mediaVideoView3DChanged?.Invoke(_base, EventArgs.Empty);

                    }

                    _base._mediaVideoCropChanged(_base, EventArgs.Empty);
                    if (_base._hasVideo)
                    {
                        _base._display.Invalidate();
                        if (_base._mediaVideoBoundsChanged != null)
                        {
                            Application.DoEvents();
                            _base._mediaVideoBoundsChanged(_base, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        // Video Overlay

        /// <summary>
        /// Provides access to the video overlay settings of the player (for example, Player.Video.Overlay.Set).
        /// </summary>
        public VideoOverlay Overlay
        {
            get
            {
                if (_videoOverlayClass == null) _videoOverlayClass = new VideoOverlay(_base);
                return _videoOverlayClass;
            }
        }

        ///// <summary>
        ///// Sets the video overlay of the player, an image that is alpha-blended with the video displayed by the player. See also: Player.Video.SetOverlayPresets, Player.Video.UpdateOverlay and Player.Video.RemoveOverlay.
        ///// </summary>
        ///// <param name="image">The image to use as the video overlay of the player.</param>
        ///// <param name="placement">Specifies the relative location and size of the overlay.</param>
        ///// <param name="transparencyKey">Source color key. Any pixels in the overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        ///// <param name="opacity">Alpha blending value. The opacity level of the overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        //public int SetOverlay(Image image, ImagePlacement placement, Color transparencyKey, float opacity)
        //{
        //    if (placement == ImagePlacement.Custom) _base._lastError = HResult.E_INVALIDARG;
        //    else _base.AV_SetVideoOverlay(image, placement, RectangleF.Empty, transparencyKey, opacity, true);
        //    return (int)_base._lastError;
        //}

        ///// <summary>
        ///// Sets the video overlay of the player, an image that is alpha-blended with the video displayed by the player. See also: Player.Video.SetOverlayPresets, Player.Video.UpdateOverlay and Player.Video.RemoveOverlay.
        ///// </summary>
        ///// <param name="image">The image to use as the video overlay of the player.</param>
        ///// <param name="placement">Specifies the relative location and size of the overlay.</param>
        ///// <param name="bounds">Specifies the relative location and size of the overlay if the placement parameter is set to ImagePlacement.Custom. The location and size of the video image is indicated by {0, 0, 1, 1}. Use RectangleF.Empty to leave this setting unchanged.</param>
        ///// <param name="transparencyKey">Source color key. Any pixels in the overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        ///// <param name="opacity">Alpha blending value. The opacity level of the overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        ///// <param name="hold">A value that indicates whether the overlay is used with all subsequent videos until the setting is changed or removed (value = true), or only with the current or next video (value = false).</param>
        //public int SetOverlay(Image image, ImagePlacement placement, RectangleF bounds, Color transparencyKey, float opacity, bool hold)
        //{
        //    _base.AV_SetVideoOverlay(image, placement, bounds, transparencyKey, opacity, hold);
        //    return (int)_base._lastError;
        //}

        ///// <summary>
        ///// Sets the size and margins of the video overlay corner presets (such as ImagePlacement.TopLeftSmall). See also: Player.Video.SetOverlay.
        ///// </summary>
        ///// <param name="smallSize">Sets the relative size of the video overlay for the small size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.10).</param>
        ///// <param name="mediumSize">Sets the relative size to the video image for the medium size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.15).</param>
        ///// <param name="largeSize">Sets the relative size to the video image for the large size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.20).</param>
        ///// <param name="horizontalMargins">Sets the size (in pixels) of the horizontal (left and right) margins between the overlay and the edge of the video. Values zero or greater or -1 to leave this setting unchanged (default: 8).</param>
        ///// <param name="verticalMargins">Sets the size (in pixels) of the vertical (top and bottom) margins between the overlay and the edge of the video. Values zero or greater or -1 to leave this setting unchanged (default: 8).</param>
        //public int SetOverlayPresets(float smallSize, float mediumSize, float largeSize, int horizontalMargins, int verticalMargins)
        //{
        //    if ((smallSize == -1 || smallSize > 0.01f) && (mediumSize == -1 || mediumSize > 0.01f) && (largeSize == -1 || largeSize > 0.01f)) // negative margins (except -1) possible
        //    {
        //        if (smallSize != -1)
        //        {
        //            _base._IMAGE_OVERLAY_SMALL = smallSize;
        //            _base._IMAGE_OVERLAY_SMALL2 = 1.0f - smallSize;
        //        }

        //        if (mediumSize != -1)
        //        {
        //            _base._IMAGE_OVERLAY_MEDIUM = mediumSize;
        //            _base._IMAGE_OVERLAY_MEDIUM2 = 1.0f - mediumSize;
        //        }

        //        if (largeSize != -1)
        //        {
        //            _base._IMAGE_OVERLAY_LARGE = largeSize;
        //            _base._IMAGE_OVERLAY_LARGE2 = 1.0f - largeSize;
        //        }

        //        if (horizontalMargins != -1)
        //        {
        //            _base._IMAGE_OVERLAY_MARGIN_HORIZONTAL = horizontalMargins;
        //            _base._IMAGE_OVERLAY_MARGIN_HORIZONTAL2 = 2.0f * horizontalMargins;
        //        }

        //        if (verticalMargins != -1)
        //        {
        //            _base._IMAGE_OVERLAY_MARGIN_VERTICAL = verticalMargins;
        //            _base._IMAGE_OVERLAY_MARGIN_VERTICAL2 = 2.0f * verticalMargins;
        //        }

        //        _base.AV_ShowVideoOverlay();
        //        _base._lastError = NO_ERROR;
        //    }
        //    else _base._lastError = HResult.E_INVALIDARG;

        //    return (int)_base._lastError;
        //}

        ///// <summary>
        ///// Updates the current video overlay settings of the player. See also: Player.Video.SetOverlay.
        ///// </summary>
        ///// <param name="bounds">Specifies the relative location and size of the video overlay and sets the image placement setting to ImagePlacement.Custom. The location and size of the video image is indicated by {0, 0, 1, 1}. Use RectangleF.Empty to leave this setting unchanged.</param>
        ///// <param name="transparencyKey">Source color key. Any pixels in the video overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        ///// <param name="opacity">Alpha blending value. The opacity level of the video overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        //public int UpdateOverlay(RectangleF bounds, Color transparencyKey, float opacity)
        //{
        //    _base.AV_UpdateVideoOverlay(bounds, transparencyKey, opacity);
        //    return (int)_base._lastError;
        //}

        ///// <summary>
        ///// Removes the video overlay from the player and releases all associated resources. See also: Player.Video.SetOverlay.
        ///// </summary>
        //public int RemoveOverlay()
        //{
        //    _base.AV_RemoveVideoOverlay();
        //    _base._lastError = NO_ERROR;
        //    return (int)_base._lastError;
        //}


        ///// <summary>
        ///// Provides access to the video recorder settings of the player (for example, Player.Video.Recorder.Start).
        ///// </summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public VideoRecorder Recorder
        //{
        //    get
        //    {
        //        if (_base._videoRecorderClass == null) _base._videoRecorderClass = new VideoRecorder(_base, false);
        //        return _base._videoRecorderClass;
        //    }
        //}

        ///// <summary>
        ///// Media Foundation Transforms Test Method.
        ///// </summary>
        //public void GetTransforms()
        //{
        //    Guid MFT_CATEGORY_VIDEO_EFFECT = new Guid(0x12e17c21, 0x532c, 0x4a6e, 0x8a, 0x1c, 0x40, 0x82, 0x5a, 0x73, 0x63, 0x97);
        //    Guid MFT_CATEGORY_AUDIO_EFFECT = new Guid(0x11064c48, 0x3648, 0x4ed0, 0x93, 0x2e, 0x05, 0xce, 0x8a, 0xc8, 0x11, 0xb7);
        //    Guid MFT_FRIENDLY_NAME_Attribute = new Guid(0x314ffbae, 0x5b41, 0x4c95, 0x9c, 0x19, 0x4e, 0x7d, 0x58, 0x6f, 0xac, 0xe3);

        //    IMFActivate[] transforms = null;
        //    int count = 0;
        //    StringBuilder name = new StringBuilder(256);
        //    int length;

        //    try
        //    {
        //        _base._lastError = MFExtern.MFTEnumEx(MFT_CATEGORY_VIDEO_EFFECT, 0, null, null, out transforms, out count);
        //    }
        //    catch { }


        //    for (int i = 0; i < count; i++)
        //    {
        //        transforms[i].GetString(MFT_FRIENDLY_NAME_Attribute, name, name.Capacity, out length);
        //        MessageBox.Show(name.ToString());
        //    }
        //}
    }

    /// <summary>
    /// A class that is used to group together the Video Overlay methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class VideoOverlay : HideObjectMembers
    {
        #region Fields (VideoOverlay Class)

        private Player _base;

		#endregion

		internal VideoOverlay(Player player)
		{
            _base = player;
        }

        /// <summary>
        /// Sets the video overlay of the player, an image that is alpha-blended with the video displayed by the player.
        /// </summary>
        /// <param name="image">The image to use as the video overlay of the player.</param>
        /// <param name="placement">Specifies the relative location and size of the overlay.</param>
        /// <param name="transparencyKey">Source color key. Any pixels in the overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        /// <param name="opacity">Alpha blending value. The opacity level of the overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        public int Set(Image image, ImagePlacement placement, Color transparencyKey, float opacity)
        {
            if (placement == ImagePlacement.Custom) _base._lastError = HResult.E_INVALIDARG;
            else _base.AV_SetVideoOverlay(image, placement, RectangleF.Empty, transparencyKey, opacity, true);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Sets the video overlay of the player, an image that is alpha-blended with the video displayed by the player.
        /// </summary>
        /// <param name="image">The image to use as the video overlay of the player.</param>
        /// <param name="placement">Specifies the relative location and size of the overlay.</param>
        /// <param name="bounds">Specifies the relative location and size of the overlay if the placement parameter is set to ImagePlacement.Custom. The location and size of the video image is indicated by {0, 0, 1, 1}. Use RectangleF.Empty to leave this setting unchanged.</param>
        /// <param name="transparencyKey">Source color key. Any pixels in the overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        /// <param name="opacity">Alpha blending value. The opacity level of the overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        /// <param name="hold">A value that indicates whether the overlay is used with all subsequent videos until the setting is changed or removed (value = true), or only with the current or next video (value = false).</param>
        public int Set(Image image, ImagePlacement placement, RectangleF bounds, Color transparencyKey, float opacity, bool hold)
        {
            _base.AV_SetVideoOverlay(image, placement, bounds, transparencyKey, opacity, hold);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Sets the size and margins of the video overlay corner presets (such as ImagePlacement.TopLeftSmall).
        /// </summary>
        /// <param name="smallSize">Sets the relative size of the video overlay for the small size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.10).</param>
        /// <param name="mediumSize">Sets the relative size to the video image for the medium size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.15).</param>
        /// <param name="largeSize">Sets the relative size to the video image for the large size presets. Values greater than zero or -1 to leave this setting unchanged (default: 0.20).</param>
        /// <param name="horizontalMargins">Sets the size (in pixels) of the horizontal (left and right) margins between the overlay and the edge of the video. Values zero or greater or -1 to leave this setting unchanged (default: 8).</param>
        /// <param name="verticalMargins">Sets the size (in pixels) of the vertical (top and bottom) margins between the overlay and the edge of the video. Values zero or greater or -1 to leave this setting unchanged (default: 8).</param>
        public int SetPresets(float smallSize, float mediumSize, float largeSize, int horizontalMargins, int verticalMargins)
        {
            if ((smallSize == -1 || smallSize > 0.01f) && (mediumSize == -1 || mediumSize > 0.01f) && (largeSize == -1 || largeSize > 0.01f)) // negative margins (except -1) possible
            {
                if (smallSize != -1)
                {
                    _base._IMAGE_OVERLAY_SMALL = smallSize;
                    _base._IMAGE_OVERLAY_SMALL2 = 1.0f - smallSize;
                }

                if (mediumSize != -1)
                {
                    _base._IMAGE_OVERLAY_MEDIUM = mediumSize;
                    _base._IMAGE_OVERLAY_MEDIUM2 = 1.0f - mediumSize;
                }

                if (largeSize != -1)
                {
                    _base._IMAGE_OVERLAY_LARGE = largeSize;
                    _base._IMAGE_OVERLAY_LARGE2 = 1.0f - largeSize;
                }

                if (horizontalMargins != -1)
                {
                    _base._IMAGE_OVERLAY_MARGIN_HORIZONTAL = horizontalMargins;
                    _base._IMAGE_OVERLAY_MARGIN_HORIZONTAL2 = 2.0f * horizontalMargins;
                }

                if (verticalMargins != -1)
                {
                    _base._IMAGE_OVERLAY_MARGIN_VERTICAL = verticalMargins;
                    _base._IMAGE_OVERLAY_MARGIN_VERTICAL2 = 2.0f * verticalMargins;
                }

                _base.AV_ShowVideoOverlay();
                _base._lastError = Player.NO_ERROR;
            }
            else _base._lastError = HResult.E_INVALIDARG;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Updates the current video overlay settings of the player.
        /// </summary>
        /// <param name="bounds">Specifies the relative location and size of the video overlay and sets the image placement setting to ImagePlacement.Custom. The location and size of the video image is indicated by {0, 0, 1, 1}. Use RectangleF.Empty to leave this setting unchanged.</param>
        /// <param name="transparencyKey">Source color key. Any pixels in the video overlay that match the color key are rendered as transparent pixels. Use Color.LightGray to use the existing transparency of the image. Use Color.Empty to leave this setting unchanged.</param>
        /// <param name="opacity">Alpha blending value. The opacity level of the video overlay. Values from 0.0 (transparent) to 1.0 (opaque), inclusive. Use value -1 to leave this setting unchanged.</param>
        public int Update(RectangleF bounds, Color transparencyKey, float opacity)
        {
            _base.AV_UpdateVideoOverlay(bounds, transparencyKey, opacity);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes the video overlay from the player and releases all associated resources.
        /// </summary>
        public int Remove()
        {
            _base.AV_RemoveVideoOverlay();
            _base._lastError = Player.NO_ERROR;
            return (int)_base._lastError;
        }
    }

	#endregion

	#region Webcam Class

	/// <summary>
	/// A class that is used to group together the Webcam methods and properties of the ABX.MediaPlayer.Player class.
	/// </summary>
	[CLSCompliant(true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class Webcam : HideObjectMembers
	{
		#region Fields (Webcam Class)

		private const int NO_ERROR = 0;

		private Player _base;
		//private VideoRecorder   _recorderClass;

		#endregion


		#region Main / Playing / Device / AudioInput / Format / GetDevices / Update

		internal Webcam(Player player)
		{
			_base = player;
		}

		/// <summary>
		/// Gets a value that indicates whether a webcam is playing (including paused webcam). Use the Player.Play method to play a webcam device.
		/// </summary>
		public bool Playing
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._webcamMode;
			}
		}

		/// <summary>
		/// Gets or sets the playing webcam device. Use the Player.Play method to play a webcam device with more options. See also: Player.Webcam.GetDevices.
		/// </summary>
		public WebcamDevice Device
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._webcamDevice;
			}
			set
			{
				//if (!_base._recording)
				{
					_base.Play(value, null, null);
				}
				//else _base._lastError = HResult.MF_E_INVALIDREQUEST;
			}
		}

		/// <summary>
		/// Gets or sets (when not recording) the audio input device of the playing webcam. See also: Player.AudioInput.GetDevices.
		/// </summary>
		public AudioInputDevice AudioInput
		{
			get
			{
				_base._lastError = NO_ERROR;
				if (_base._webcamMode) return _base._micDevice;
				return null;
			}
			set
			{
				if (_base._webcamMode && !_base._recording)
				{
					_base._lastError = NO_ERROR;
					if (!((value == null && _base._micDevice == null) || (value != null && _base._micDevice != null && value._id == _base._micDevice._id)))
					{
						if (_base.pm_HasInputMeter)
						{
							_base.InputMeter_Close();
							_base.pm_InputMeterPending = true;
						}

						_base._micDevice = value;
						_base.AV_UpdateTopology();
						_base._mediaAudioInputDeviceChanged?.Invoke(_base, EventArgs.Empty);
					}
				}
				else _base._lastError = HResult.MF_E_INVALIDREQUEST;
			}
		}

		/// <summary>
		/// Gets or sets the video output format of the playing webcam. See also: Player.Webcam.GetFormats.
		/// </summary>
		public WebcamFormat Format
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._webcamFormat;
			}
			set
			{
				if (value == null) _base._lastError = HResult.E_INVALIDARG;
				else
				{
					// setting the format directly does not seem to work
					if (_base._webcamMode)
					{
						_base._lastError = NO_ERROR;
						if ((value == null && _base._webcamFormat != null) ||
						(value != null && _base._webcamFormat == null) ||
						(value._typeIndex != _base._webcamFormat._typeIndex))
						{
							_base._webcamFormat = value;
							_base.AV_UpdateTopology();
							if (_base._hasDisplay)
							{
								_base._hasVideoBounds = false;
								_base._display.Invalidate();
							}
							_base._mediaWebcamFormatChanged?.Invoke(_base, EventArgs.Empty);
						}
					}
					else _base._lastError = HResult.MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED;
				}
			}
		}

		/// <summary>
		/// Gets the number of the system's enabled webcam devices. See also: Player.Webcam.GetDevices.
		/// </summary>
		public int DeviceCount
		{
			get
			{
				HResult result;

				MFExtern.MFCreateAttributes(out IMFAttributes attributes, 1);
				attributes.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);

				result = MFExtern.MFEnumDeviceSources(attributes, out IMFActivate[] webcams, out int webcamCount);
				Marshal.ReleaseComObject(attributes);

				if (result == NO_ERROR && webcams != null)
				{
					for (int i = 0; i < webcamCount; i++)
					{
						Marshal.ReleaseComObject(webcams[i]);
					}
				}

				_base._lastError = NO_ERROR;
				return webcamCount;
			}
		}

		/// <summary>
		/// Returns the enabled webcam devices of the system or null if none are present. See also: Player.Webcam.DeviceCount.
		/// </summary>
		public WebcamDevice[] GetDevices()
		{
			WebcamDevice[] devices = null;
			HResult result;

			MFExtern.MFCreateAttributes(out IMFAttributes attributes, 1);
			attributes.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);

			result = MFExtern.MFEnumDeviceSources(attributes, out IMFActivate[] webcams, out int webcamCount);
			if (result == NO_ERROR)
			{
				if (webcams == null) result = HResult.MF_E_NO_CAPTURE_DEVICES_AVAILABLE;
				else
				{
					devices = new WebcamDevice[webcamCount];
					for (int i = 0; i < webcamCount; i++)
					{
						devices[i] = new WebcamDevice();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
						webcams[i].GetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME, _base._textBuffer1, _base._textBuffer1.Capacity, out int length);
						devices[i]._name = _base._textBuffer1.ToString();
						webcams[i].GetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, _base._textBuffer1, _base._textBuffer1.Capacity, out length);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
						devices[i]._id = _base._textBuffer1.ToString();

						Marshal.ReleaseComObject(webcams[i]);
					}
				}
			}
			Marshal.ReleaseComObject(attributes);

			_base._lastError = result;
			return devices;
		}

		/// <summary>
		/// Updates or restores the audio and video playback of the playing webcam.
		/// </summary>
		public int Update()
		{
			if (_base._webcamMode)
			{
				_base.AV_UpdateTopology();
				if (_base._hasOverlay) _base.AV_ShowOverlay();
				_base._lastError = NO_ERROR;
			}
			else _base._lastError = HResult.MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED;
			return (int)_base._lastError;
		}

		#endregion

		#region Public - SetProperty / UpdateProperty / ResetProperty / Settings / SetSettings

		private bool _webcamSourceCreated;
		private IMFMediaSource WebcamMediaSource
		{
			get
			{
				IMFMediaSource source;
				if (_base._webcamAggregated)
				{
					MFExtern.MFCreateAttributes(out IMFAttributes attributes, 2);
					attributes.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
					attributes.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, _base._webcamDevice._id);

					MFExtern.MFCreateDeviceSource(attributes, out source);
					_webcamSourceCreated = true;
					Marshal.ReleaseComObject(attributes);
				}
				else
				{
					source = _base.mf_MediaSource;
					_webcamSourceCreated = false;
				}
				return source;
			}
		}

		/// <summary>
		/// Sets the specified property of the playing webcam to the specified value or to automatic control, for example myPlayer.Webcam.SetProperty(myPlayer.Webcam.Brightness, 100, false).
		/// </summary>
		/// <param name="property">Specifies the webcam property, obtained with for example myPlayer.Webcam.Brightness.</param>
		/// <param name="value">The value to be set.</param>
		/// <param name="auto">If set to true, the value parameter is ignored and the automatic control of the property is enabled (if available).</param>
		public void SetProperty(WebcamProperty property, int value, bool auto)
		{
			if (_base._webcamMode)
			{
				if (property != null)
				{
					WebcamProperty currentProp;
					if (property._isProcAmp) currentProp = GetProcAmpProperties(property._procAmpProp);
					else currentProp = GetControlProperties(property._controlProp);

					if (currentProp.Supported)
					{
						if (auto || (value >= currentProp._min && value <= currentProp._max))
						{
							bool setProperty = true;
							if (auto)
							{
								if (currentProp._auto) setProperty = false;
							}
							else if (value == currentProp._value && !currentProp.AutoEnabled)
							{
								setProperty = false;
							}
							if (setProperty)
							{
								try
								{
									if (property._isProcAmp)
									{
#pragma warning disable IDE0019 // Use pattern matching
										IAMVideoProcAmp control = WebcamMediaSource as IAMVideoProcAmp;
#pragma warning restore IDE0019 // Use pattern matching
										if (control != null)
										{
											_base._lastError = control.Set(property._procAmpProp, value, auto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual);
											if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
										}
									}
									else
									{
#pragma warning disable IDE0019 // Use pattern matching
										IAMCameraControl control = WebcamMediaSource as IAMCameraControl;
#pragma warning restore IDE0019 // Use pattern matching
										if (control != null)
										{
											_base._lastError = control.Set(property._controlProp, value, auto ? CameraControlFlags.Auto : CameraControlFlags.Manual);
											if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
										}
									}
									if (_base._lastError == NO_ERROR)
									{
										if (auto) property._auto = true;
										else property._value = value;
									}
								}
								catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
							}
							else _base._lastError = NO_ERROR;
						}
						else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
					}
					else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
				}
				else _base._lastError = HResult.E_INVALIDARG;
			}
			else _base._lastError = HResult.MF_E_INVALIDREQUEST;
		}

		/// <summary>
		/// Updates the values in the specified (previously obtained) property data of the playing webcam.
		/// </summary>
		/// <param name="property">Specifies the webcam property data to update, previously obtained with for example myPlayer.Webcam.Brightness.</param>
		public void UpdatePropertyData(WebcamProperty property)
		{
			if (_base._webcamMode)
			{
				if (property != null)
				{
					WebcamProperty currentProperty;

					if (property._isProcAmp) currentProperty = GetProcAmpProperties(property._procAmpProp);
					else currentProperty = GetControlProperties(property._controlProp);

					property._value = currentProperty._value;
					property._auto = currentProperty._auto;

					_base._lastError = NO_ERROR;
				}
				else _base._lastError = HResult.E_INVALIDARG;
			}
			else _base._lastError = HResult.MF_E_INVALIDREQUEST;
		}

		/// <summary>
		/// Sets the specified property of the playing webcam to the default value or (if available) to automatic control, for example myPlayer.Webcam.ResetProperty(myPlayer.Webcam.Brightness).
		/// </summary>
		/// <param name="property">Specifies the webcam property, obtained with for example myPlayer.Webcam.Brightness.</param>
		public void ResetProperty(WebcamProperty property)
		{
			SetProperty(property, property._default, property._autoSupport);
		}

		/// <summary>
		/// Gets or sets all the properties, including the video output format, of the playing webcam. For use with save and restore functions. See also: Player.Webcam.ApplySettings.
		/// </summary>
		public WebcamSettings Settings
		{
			get
			{
				WebcamSettings settings = null;
				if (_base._webcamMode)
				{
					settings = new WebcamSettings
					{
						_webcamName = _base._webcamDevice.Name,
						_format = _base._webcamFormat
					};

					WebcamProperty props = GetProcAmpProperties(VideoProcAmpProperty.BacklightCompensation);
					settings._backlight = props._value;
					settings._autoBacklight = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Brightness);
					settings._brightness = props._value;
					settings._autoBrightness = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.ColorEnable);
					settings._colorEnable = props._value;
					settings._autoColorEnable = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Contrast);
					settings._contrast = props._value;
					settings._autoContrast = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Gain);
					settings._gain = props._value;
					settings._autoGain = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Gamma);
					settings._gamma = props._value;
					settings._autoGamma = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Hue);
					settings._hue = props._value;
					settings._autoHue = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.PowerLineFrequency);
					settings._powerLine = props._value;
					settings._autoPowerLine = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Saturation);
					settings._saturation = props._value;
					settings._autoSaturation = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.Sharpness);
					settings._sharpness = props._value;
					settings._autoSharpness = props._auto;

					props = GetProcAmpProperties(VideoProcAmpProperty.WhiteBalance);
					settings._whiteBalance = props._value;
					settings._autoWhiteBalance = props._auto;


					props = GetControlProperties(CameraControlProperty.Exposure);
					settings._exposure = props._value;
					settings._autoExposure = props._auto;

					props = GetControlProperties(CameraControlProperty.Flash);
					settings._flash = props._value;
					settings._autoFlash = props._auto;

					props = GetControlProperties(CameraControlProperty.Focus);
					settings._focus = props._value;
					settings._autoFocus = props._auto;

					props = GetControlProperties(CameraControlProperty.Iris);
					settings._iris = props._value;
					settings._autoIris = props._auto;

					props = GetControlProperties(CameraControlProperty.Pan);
					settings._pan = props._value;
					settings._autoPan = props._auto;

					props = GetControlProperties(CameraControlProperty.Roll);
					settings._roll = props._value;
					settings._autoRoll = props._auto;

					props = GetControlProperties(CameraControlProperty.Tilt);
					settings._tilt = props._value;
					settings._autoTilt = props._auto;

					props = GetControlProperties(CameraControlProperty.Zoom);
					settings._zoom = props._value;
					settings._autoZoom = props._auto;

					_base._lastError = NO_ERROR;
				}
				else _base._lastError = HResult.MF_E_INVALIDREQUEST;
				return settings;
			}
			set
			{
				ApplySettings(value, true, true, true);
			}
		}

		/// <summary>
		/// Applies previously obtained (saved) webcam settings selectively to the playing webcam. See also: Player.Webcam.Settings.
		/// </summary>
		/// <param name="settings">The settings to be applied to the playing webcam. The settings must have been obtained earlier from the webcam, settings from other webcams cannot be used.</param>
		/// <param name="format">A value that indicates whether to apply the webcam video output format (size and fps).</param>
		/// /// <param name="procAmp">A value that indicates whether to apply the webcam video quality properties (such as brightness and contrast).</param>
		/// <param name="control">A value that indicates whether to apply the webcam camera control properties (such as focus and zoom).</param>
		public int ApplySettings(WebcamSettings settings, bool format, bool procAmp, bool control)
		{
			if (_base._webcamMode)
			{
				if (settings != null && string.Compare(settings._webcamName, _base._webcamDevice.Name, false) == 0)
				{
					WebcamProperty props;

					//if (format) { Format = settings._format; } see below

					if (procAmp)
					{
						props = GetProcAmpProperties(VideoProcAmpProperty.BacklightCompensation);
						SetProperty(props, settings._backlight, settings._autoBacklight);

						props = GetProcAmpProperties(VideoProcAmpProperty.Brightness);
						SetProperty(props, settings._brightness, settings._autoBrightness);

						props = GetProcAmpProperties(VideoProcAmpProperty.ColorEnable);
						SetProperty(props, settings._colorEnable, settings._autoColorEnable);

						props = GetProcAmpProperties(VideoProcAmpProperty.Contrast);
						SetProperty(props, settings._contrast, settings._autoContrast);

						props = GetProcAmpProperties(VideoProcAmpProperty.Gain);
						SetProperty(props, settings._gain, settings._autoGain);

						props = GetProcAmpProperties(VideoProcAmpProperty.Gamma);
						SetProperty(props, settings._gamma, settings._autoGamma);

						props = GetProcAmpProperties(VideoProcAmpProperty.Hue);
						SetProperty(props, settings._hue, settings._autoHue);

						props = GetProcAmpProperties(VideoProcAmpProperty.PowerLineFrequency);
						SetProperty(props, settings._powerLine, settings._autoPowerLine);

						props = GetProcAmpProperties(VideoProcAmpProperty.Saturation);
						SetProperty(props, settings._backlight, settings._autoBacklight);

						props = GetProcAmpProperties(VideoProcAmpProperty.Sharpness);
						SetProperty(props, settings._sharpness, settings._autoSharpness);

						props = GetProcAmpProperties(VideoProcAmpProperty.WhiteBalance);
						SetProperty(props, settings._whiteBalance, settings._autoWhiteBalance);
					}

					if (control)
					{
						props = GetControlProperties(CameraControlProperty.Exposure);
						SetProperty(props, settings._exposure, settings._autoExposure);

						props = GetControlProperties(CameraControlProperty.Flash);
						SetProperty(props, settings._flash, settings._autoFlash);

						props = GetControlProperties(CameraControlProperty.Focus);
						SetProperty(props, settings._focus, settings._autoFocus);

						props = GetControlProperties(CameraControlProperty.Iris);
						SetProperty(props, settings._iris, settings._autoIris);

						props = GetControlProperties(CameraControlProperty.Pan);
						SetProperty(props, settings._pan, settings._autoPan);

						props = GetControlProperties(CameraControlProperty.Roll);
						SetProperty(props, settings._roll, settings._autoRoll);

						props = GetControlProperties(CameraControlProperty.Tilt);
						SetProperty(props, settings._tilt, settings._autoTilt);

						props = GetControlProperties(CameraControlProperty.Zoom);
						SetProperty(props, settings._zoom, settings._autoZoom);
					}

					if (format) { Format = settings._format; }

					_base._lastError = NO_ERROR;
				}
				else _base._lastError = HResult.E_INVALIDARG;
			}
			else _base._lastError = HResult.MF_E_INVALIDREQUEST;
			return (int)_base._lastError;
		}

		#endregion


		#region Private - Get/Set Video Control Properties / ProcAmp Properties

		internal WebcamProperty GetControlProperties(CameraControlProperty property)
		{
			HResult result = HResult.ERROR_NOT_READY;

			WebcamProperty settings = new WebcamProperty
			{
				_name = property.ToString(),
				_controlProp = property
			};

			if (_base._webcamMode)
			{
#pragma warning disable IDE0019 // Use pattern matching
				IAMCameraControl control = WebcamMediaSource as IAMCameraControl;
#pragma warning restore IDE0019 // Use pattern matching
				if (control != null)
				{
					result = control.GetRange(property, out settings._min, out settings._max, out settings._step, out settings._default, out CameraControlFlags flags);

					if (result == NO_ERROR)
					{
						settings._supported = (flags & CameraControlFlags.Manual) != 0;
						settings._autoSupport = (flags & CameraControlFlags.Auto) != 0;

						control.Get(property, out settings._value, out flags);
						settings._auto = (flags & CameraControlFlags.Auto) != 0;
					}

					if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
				}
			}
			_base._lastError = result;
			return settings;
		}

		internal void SetControlProperties(CameraControlProperty property, WebcamProperty value)
		{
			HResult result = HResult.ERROR_NOT_READY;
			if (_base._webcamMode)
			{
				if (value == null || value._isProcAmp || value._controlProp != property)
				{
					result = HResult.E_INVALIDARG;
				}
				else
				{
					WebcamProperty settings = GetControlProperties(property);
					if (!settings._supported) result = HResult.MF_E_NOT_AVAILABLE;
					else if (value._auto && settings._auto) result = NO_ERROR;
					else if (!value._auto && (value._value < settings._min || value._value > settings._max)) result = HResult.MF_E_OUT_OF_RANGE;

					if (result == HResult.ERROR_NOT_READY)
					{
						try
						{
#pragma warning disable IDE0019 // Use pattern matching
							IAMCameraControl control = WebcamMediaSource as IAMCameraControl;
#pragma warning restore IDE0019 // Use pattern matching
							if (control != null)
							{
								result = control.Set(property, value._value, value._auto ? CameraControlFlags.Auto : CameraControlFlags.Manual);
								if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
							}
						}
						catch (Exception e) { result = (HResult)Marshal.GetHRForException(e); }
					}
				}
			}
			_base._lastError = result;
		}

		internal WebcamProperty GetProcAmpProperties(VideoProcAmpProperty property)
		{
			HResult result = HResult.ERROR_NOT_READY;

			WebcamProperty settings = new WebcamProperty
			{
				_name = property.ToString(),
				_procAmpProp = property,
				_isProcAmp = true
			};

			if (_base._webcamMode)
			{
#pragma warning disable IDE0019 // Use pattern matching
				IAMVideoProcAmp control = WebcamMediaSource as IAMVideoProcAmp;
#pragma warning restore IDE0019 // Use pattern matching
				if (control != null)
				{
					result = control.GetRange(property, out settings._min, out settings._max, out settings._step, out settings._default, out VideoProcAmpFlags flags);

					if (result == NO_ERROR)
					{
						settings._supported = (flags & VideoProcAmpFlags.Manual) != 0;
						settings._autoSupport = (flags & VideoProcAmpFlags.Auto) != 0;

						control.Get(property, out settings._value, out flags);
						settings._auto = (flags & VideoProcAmpFlags.Auto) != 0;
					}
					if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
				}
			}
			_base._lastError = result;
			return settings;
		}

		internal void SetProcAmpProperties(VideoProcAmpProperty property, WebcamProperty value)
		{
			HResult result = HResult.ERROR_NOT_READY;
			if (_base._webcamMode)
			{
				if (value == null || !value._isProcAmp || value._procAmpProp != property)
				{
					result = HResult.E_INVALIDARG;
				}
				else
				{
					WebcamProperty props = GetProcAmpProperties(property);
					if (!props._supported) result = HResult.MF_E_NOT_AVAILABLE;
					else if (value._auto && props._auto) result = NO_ERROR;
					else if (!value._auto && (value._value < props._min || value._value > props._max)) result = HResult.MF_E_OUT_OF_RANGE;

					if (result == HResult.ERROR_NOT_READY)
					{
						try
						{
#pragma warning disable IDE0019 // Use pattern matching
							IAMVideoProcAmp control = WebcamMediaSource as IAMVideoProcAmp;
#pragma warning restore IDE0019 // Use pattern matching
							if (control != null)
							{
								result = control.Set(property, value._value, value._auto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual);
								if (_webcamSourceCreated) Marshal.ReleaseComObject(control);
							}
						}
						catch (Exception e) { result = (HResult)Marshal.GetHRForException(e); }
					}
				}
			}
			_base._lastError = result;
		}

		#endregion

		#region Public - Get/Set Video Control Properties

		/// <summary>
		/// Gets or sets the exposure property (if supported) of the playing webcam. Values are in log base 2 seconds, for values less than zero the exposure time is 1/2^n seconds (eg. -3 = 1/8), and for values zero or above the exposure time is 2^n seconds (eg. 0 = 1 and 2 = 4). See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Exposure
		{
			get { return GetControlProperties(CameraControlProperty.Exposure); }
			set { SetControlProperties(CameraControlProperty.Exposure, value); }
		}

		/// <summary>
		/// Gets or sets the flash property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Flash
		{
			get { return GetControlProperties(CameraControlProperty.Flash); }
			set { SetControlProperties(CameraControlProperty.Flash, value); }
		}

		/// <summary>
		/// Gets or sets the focus property (if supported) of the playing webcam. Values represent the distance to the optimally focused target, in millimeters. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Focus
		{
			get { return GetControlProperties(CameraControlProperty.Focus); }
			set { SetControlProperties(CameraControlProperty.Focus, value); }
		}

		/// <summary>
		/// Gets or sets the iris property (if supported) of the playing webcam. Values are in units of f-stop * 10 (a larger f-stop value will result in darker images). See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Iris
		{
			get { return GetControlProperties(CameraControlProperty.Iris); }
			set { SetControlProperties(CameraControlProperty.Iris, value); }
		}

		/// <summary>
		/// Gets or sets the pan property (if supported) of the playing webcam. Values are in degrees. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Pan
		{
			get { return GetControlProperties(CameraControlProperty.Pan); }
			set { SetControlProperties(CameraControlProperty.Pan, value); }
		}

		/// <summary>
		/// Gets or sets the roll property (if supported) of the playing webcam. Values are in degrees.  See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Roll
		{
			get { return GetControlProperties(CameraControlProperty.Roll); }
			set { SetControlProperties(CameraControlProperty.Roll, value); }
		}

		/// <summary>
		/// Gets or sets the tilt property (if supported) of the playing webcam. Values are in degrees. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Tilt
		{
			get { return GetControlProperties(CameraControlProperty.Tilt); }
			set { SetControlProperties(CameraControlProperty.Tilt, value); }
		}

		/// <summary>
		/// Gets or sets the zoom property (if supported) of the playing webcam. Values are in millimeters.  See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Zoom
		{
			get { return GetControlProperties(CameraControlProperty.Zoom); }
			set { SetControlProperties(CameraControlProperty.Zoom, value); }
		}

		#endregion

		#region Public - Get/SetVideo ProcAmp Properties

		/// <summary>
		/// Gets or sets the backlight compensation property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty BacklightCompensation
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.BacklightCompensation); }
			set { SetProcAmpProperties(VideoProcAmpProperty.BacklightCompensation, value); }
		}

		/// <summary>
		/// Gets or sets the brightness property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty and Player.Video.Brightness.
		/// </summary>
		public WebcamProperty Brightness
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Brightness); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Brightness, value); }
		}

		/// <summary>
		/// Gets or sets the color enable property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty ColorEnable
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.ColorEnable); }
			set { SetProcAmpProperties(VideoProcAmpProperty.ColorEnable, value); }
		}

		/// <summary>
		/// Gets or sets the contrast property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty and Player.Video.Contrast.
		/// </summary>
		public WebcamProperty Contrast
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Contrast); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Contrast, value); }
		}

		/// <summary>
		/// Gets or sets the gain property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Gain
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Gain); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Gain, value); }
		}

		/// <summary>
		/// Gets or sets the gamma property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Gamma
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Gamma); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Gamma, value); }
		}

		/// <summary>
		/// Gets or sets the hue property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty and Player.Video.Hue.
		/// </summary>
		public WebcamProperty Hue
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Hue); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Hue, value); }
		}

		/// <summary>
		/// Gets or sets the power line frequency property (if supported) of the playing webcam. Values: 0 = disabled, 1 = 50Hz, 2 = 60Hz, 3 = auto). See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty PowerLineFrequency
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.PowerLineFrequency); }
			set { SetProcAmpProperties(VideoProcAmpProperty.PowerLineFrequency, value); }
		}

		/// <summary>
		/// Gets or sets the saturation property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty and Player.Video.Saturation.
		/// </summary>
		public WebcamProperty Saturation
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Saturation); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Saturation, value); }
		}

		/// <summary>
		/// Gets or sets the sharpness property (if supported) of the playing webcam. See also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty Sharpness
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.Sharpness); }
			set { SetProcAmpProperties(VideoProcAmpProperty.Sharpness, value); }
		}

		/// <summary>
		/// Gets or sets the white balance temperature property (if supported) of the playing webcam. Ssee also: Player.Webcam.SetProperty.
		/// </summary>
		public WebcamProperty WhiteBalance
		{
			get { return GetProcAmpProperties(VideoProcAmpProperty.WhiteBalance); }
			set { SetProcAmpProperties(VideoProcAmpProperty.WhiteBalance, value); }
		}

		#endregion


		#region Private - Get Video Output Format

		private WebcamFormat[] GetWebcamFormats(string webcamId, bool filter, bool exact, int minWidth, int minHeight, float minFrameRate)
		{
			List<WebcamFormat> list = null;

			HResult result = GetMediaSource(webcamId, out IMFMediaSource source);
			if (result == NO_ERROR)
			{
				result = MFExtern.MFCreateSourceReaderFromMediaSource(source, null, out IMFSourceReader reader);
				if (result == NO_ERROR)
				{
					HResult readResult = NO_ERROR;

					int streamIndex = 0;
					int typeIndex = 0;

					float frameRate = 0;
					bool match;

					list = new List<WebcamFormat>(250);

					while (readResult == NO_ERROR)
					{
						readResult = reader.GetNativeMediaType(streamIndex, typeIndex, out IMFMediaType type);
						if (readResult == NO_ERROR)
						{
							MFExtern.MFGetAttributeRatio(type, MFAttributesClsid.MF_MT_FRAME_RATE, out int num, out int denum);
							if (denum > 0) frameRate = (float)num / denum;
							MFExtern.MFGetAttributeRatio(type, MFAttributesClsid.MF_MT_FRAME_SIZE, out int width, out int height);

							match = true;
							if (filter)
							{
								if (exact)
								{
									if ((minWidth != -1 && width != minWidth) || (minHeight != -1 && height != minHeight) || (minFrameRate != -1 && frameRate != minFrameRate)) match = false;
								}
								else if ((minWidth != -1 && width < minWidth) || (minHeight != -1 && height < minHeight) || (minFrameRate != -1 && frameRate < minFrameRate)) match = false;
							}

							if (match && !FormatExists(list, width, height, frameRate))
							{
								list.Add(new WebcamFormat(streamIndex, typeIndex, width, height, frameRate));
							}

							typeIndex++;
							Marshal.ReleaseComObject(type);
						}
						// read formats of 1 track (stream) only - can't switch tracks (?)
						//else if (readResult == HResult.MF_E_NO_MORE_TYPES)
						//{
						//    readResult = NO_ERROR;
						//    streamIndex++;
						//    typeIndex = 0;
						//}
					}
					if (reader != null) Marshal.ReleaseComObject(reader);
				}
				if (source != null) Marshal.ReleaseComObject(source);
			}

			_base._lastError = result;
			return (list == null || list.Count == 0) ? null : list.ToArray();
		}

		private static bool FormatExists(List<WebcamFormat> list, int width, int height, float frameRate)
		{
			bool exists = false;
			int length = list.Count;

			for (int i = 0; i < length; i++)
			{
				if (list[i]._width == width && list[i]._height == height && list[i]._frameRate == frameRate)
				{
					exists = true;
					break;
				}
			}
			return exists;
		}

		private static HResult GetMediaSource(string webcamId, out IMFMediaSource source)
		{
			MFExtern.MFCreateAttributes(out IMFAttributes attributes, 2);
			attributes.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
			attributes.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, webcamId);

			HResult result = MFExtern.MFCreateDeviceSource(attributes, out source);
			if ((uint)result == 0xC00D36E6) result = HResult.ERROR_DEVICE_NOT_CONNECTED;

			Marshal.ReleaseComObject(attributes);
			return result;
		}

		internal WebcamFormat GetHighFormat(WebcamDevice webcam, bool photo)
		{
			WebcamFormat format = null;

			WebcamFormat[] formats = GetWebcamFormats(webcam._id, false, false, 0, 0, 0);
			if (formats != null)
			{
				format = formats[0];
				int count = formats.Length;
				int frameRate = photo ? 1 : 15;

				for (int i = 1; i < count; i++)
				{
					if (formats[i]._width >= format._width &&
						formats[i]._height >= format._height &&
						formats[i]._frameRate >= format._frameRate)
					{
						format = formats[i];
					}
					else if (formats[i]._width > format._width &&
						formats[i]._height > format._height &&
						formats[i]._frameRate >= frameRate)
					{
						format = formats[i];
					}
				}
			}
			return format;
		}

		internal WebcamFormat GetLowFormat(WebcamDevice webcam)
		{
			WebcamFormat format = null;

			WebcamFormat[] formats = GetWebcamFormats(webcam._id, false, false, 0, 0, 0);
			if (formats != null)
			{
				format = formats[0];
				int count = formats.Length;

				for (int i = 1; i < count; i++)
				{
					if (formats[i]._width <= format._width &&
						formats[i]._height <= format._height &&
						formats[i]._height >= 100 &&
						formats[i]._frameRate <= format._frameRate &&
						formats[i]._frameRate >= 15)
						format = formats[i];
				}
			}
			return format;
		}

		#endregion

		#region Public - Get Video Output Formats

		/// <summary>
		/// Returns the available video output formats of the playing webcam. The formats can be used with the Player.Webcam.Format and Player.Play methods.
		/// </summary>
		public WebcamFormat[] GetFormats()
		{
			if (!_base._webcamMode)
			{
				_base._lastError = HResult.MF_E_INVALIDREQUEST;
				return null;
			}
			return GetWebcamFormats(_base._webcamDevice._id, false, false, 0, 0, 0);
		}

		/// <summary>
		/// Returns the available video output formats of the specified webcam. The formats can be used with the Player.Play methods for webcams.
		/// </summary>
		/// <param name="webcam">The webcam whose video output formats are to be obtained.</param>
		public WebcamFormat[] GetFormats(WebcamDevice webcam)
		{
			return GetWebcamFormats(webcam._id, false, false, 0, 0, 0);
		}

		/// <summary>
		/// Returns the available video output formats of the playing webcam that match the specified values. The formats can be used with the Player.Webcam.Format and Player.Play methods.
		/// </summary>
		/// <param name="exact">A value that indicates whether the specified values must exactly match the webcam formats or whether they are minimum values.</param>
		/// <param name="width">The (minimum) width of the video frames. Use -1 to ignore this parameter.</param>
		/// <param name="height">The (minimum) height of the video frames. Use -1 to ignore this parameter.</param>
		/// <param name="frameRate">The (minimum) frame rate of the video output format. Use -1 to ignore this parameter.</param>
		public WebcamFormat[] GetFormats(bool exact, int width, int height, float frameRate)
		{
			if (!_base._webcamMode)
			{
				_base._lastError = HResult.MF_E_INVALIDREQUEST;
				return null;
			}
			return GetWebcamFormats(_base._webcamDevice._id, true, exact, width, height, frameRate);
		}

		/// <summary>
		/// Returns the available video output formats of the specified webcam that match the specified values or null if none are found. The formats can be used with the Player.Play methods for webcams.
		/// </summary>
		/// <param name="webcam">The webcam whose video output formats are to be obtained.</param>
		/// <param name="exact">A value that indicates whether the specified values must exactly match the webcam formats or whether they are minimum values.</param>
		/// <param name="width">The (minimum) width of the video frames. Use -1 to ignore this parameter.</param>
		/// <param name="height">The (minimum) height of the video frames. Use -1 to ignore this parameter.</param>
		/// <param name="frameRate">The (minimum) frame rate of the video output format. Use -1 to ignore this parameter.</param>
		public WebcamFormat[] GetFormats(WebcamDevice webcam, bool exact, int width, int height, float frameRate)
		{
			return GetWebcamFormats(webcam._id, true, exact, width, height, frameRate);
		}

		#endregion


		//#region Public - Video Recorder

		///// <summary>
		///// Provides access to the webcam video recorder settings of the player (for example, Player.Webcam.Recorder.Start).
		///// </summary>
		//public VideoRecorder Recorder
		//{
		//    get
		//    {
		//        if (_base._webcamRecorderClass == null) _base._webcamRecorderClass = new VideoRecorder(_base, true);
		//        return _base._webcamRecorderClass;
		//    }
		//}

		//#endregion

	}

	#endregion

	#region Recorder Class

	/// <summary>
	/// A class that is used to group together the Recorder methods and properties of the ABX.MediaPlayer.Player class.
	/// </summary>
	[CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Recorder : HideObjectMembers
    {
        #region Fields (Recorder Class)

        private const int           NO_ERROR = 0;

        private Player              _base;

        private class MF_RecorderCallBack : IMFCaptureEngineOnEventCallback
        {
            #region Fields

            private Guid MF_CAPTURE_ENGINE_ERROR = new Guid(0x46b89fc6, 0x33cc, 0x4399, 0x9d, 0xad, 0x78, 0x4d, 0xe7, 0x7d, 0x58, 0x7c);

            private Recorder _base;
            private delegate void EndOfMediaDelegate();
            private EndOfMediaDelegate CallEndOfMedia;

            #endregion

            public MF_RecorderCallBack(Recorder recorder)
            {
                _base = recorder;
                CallEndOfMedia = new EndOfMediaDelegate(_base._base.AV_EndOfMedia);
            }

            public void Dispose()
            {
                try
                {
                    // TODO - reactivate 'old' callback?
                    if (_base._resetAfterStop)
                    {
                        _base._base.AV_UpdateTopology();
                        if (_base._base._hasOverlay) _base._base.AV_ShowOverlay();
                    }

					_base = null;
					CallEndOfMedia = null;
				}
                catch { /* ignored */ }
            }

            public HResult OnEvent(IMFMediaEvent mediaEvent)
            {
                if (_base.AwaitCallBack)
                {
                    mediaEvent.GetStatus(out _base.ErrorFromCallback);

                    _base.AwaitCallBack = false;
                    _base._base.WaitForEvent.Set();
                }
                else
                {
                    try
                    {
                        mediaEvent.GetStatus(out HResult status);

						if (status != NO_ERROR && status != HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED)
						{
							mediaEvent.GetExtendedType(out Guid mediaEventType);
                            if (mediaEventType == MF_CAPTURE_ENGINE_ERROR)
                            {
                                _base._base._lastError = status;
                                Control control = _base._base._display;
                                if (control == null)
                                {
                                    FormCollection forms = Application.OpenForms;
                                    if (forms != null && forms.Count > 0) control = forms[0];
                                }
                                if (control != null) control.BeginInvoke(CallEndOfMedia);
                                else _base._base.AV_EndOfMedia();
                            }
						}
                    }
                    catch { /* ignored */ }
                }
                return HResult.S_OK;
            }
        }
        private MF_RecorderCallBack _callBack;
        private bool                AwaitCallBack;
        private HResult             ErrorFromCallback;

        private IMFCaptureEngine    _recorder;
        private AudioFormat         _audioFormat        = Player.DEFAULT_RECORDER_AUDIO_FORMAT;
        private AudioBitRate        _audioBitRate       = Player.DEFAULT_RECORDER_AUDIO_BITRATE;
        private VideoFormat         _videoFormat        = Player.DEFAULT_RECORDER_VIDEO_FORMAT;

        private string              _fileName           = string.Empty;
        private string              _folderName         = Player.DEFAULT_RECORDER_FOLDER_NAME;
        private bool                _setFileExtension   = true;
        private bool                _setMetadata        = true;
        private bool                _resetAfterStop     = false;    // restore base callback just before or after recorder stop

        private Recording           _info;
        private bool                _hasAudio;
        private int                 _audioIndex;
        private bool                _hasVideo;
        private int                 _videoIndex;

        #endregion


        #region Main

        internal Recorder(Player player)
		{
            _base = player;

			// to prevent null exceptions for 'easy' users
			_info = new Recording
			{
				_hasAudio   = _hasAudio,
				_audio      = new AudioTrack(),
				_hasVideo   = _hasVideo,
				_video      = new VideoTrack()
			};
		}

        #endregion

        #region Audio Format / Audio Bit Rate / Video Format / Auto File Extension / Info

        /// <summary>
        /// Gets or sets the audio format of the recorder. The format can be changed by the recorder when video is also recorded and the set value is not supported by the video format (default: AudioFormat.AAC (.m4a))
        /// </summary>
        public AudioFormat AudioFormat
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _audioFormat;
			}
			set
			{
                if (!_base._recording)
                {
                    _base._lastError = NO_ERROR;
                    _audioFormat = value;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
			}
		}

        /// <summary>
        /// Gets or sets the audio bit rate of the recorder. The bit rate can be changed by the recorder if the set value is not supported by the set audio format (default: Kbps_128).
        /// </summary>
        public AudioBitRate AudioBitRate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _audioBitRate;
            }
            set
            {
                if (!_base._recording)
                {
                    _base._lastError = NO_ERROR;
                    _audioBitRate = value;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
        }

        /// <summary>
        /// Gets or sets the video format of the recorder (default: VideoFormat.H264 (.mp4)).
        /// </summary>
		public VideoFormat VideoFormat
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _videoFormat;
            }
            set
            {
                if (!_base._recording)
                {
                    _base._lastError = NO_ERROR;
                    _videoFormat = value;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies whether the recorder can change the file extension of the specified file name for a recording (default: true).
        /// </summary>
        public bool AutoFileExtension
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _setFileExtension;
			}
			set
			{
				_setFileExtension = value;
				_base._lastError = NO_ERROR;
			}
		}

        /// <summary>
		/// Gets or sets a value that indicates whether the recorder adds metadata items "title" (file name), "Album Artist" (Player.Name) and "year" (current year) to new recordings (default: true).
		/// </summary>
		public bool AutoMetadata
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _setMetadata;
            }
            set
            {
                _setMetadata = value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets information about the current or most recent recording. The information may differ from the settings originally specified for the recording.
        /// </summary>
        public Recording Info
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _info;
            }
        }

        #endregion

        #region Recorder Start / Stop

        /// <summary>
        /// Start recording the devices (webcam and/or audio input) played by the player to a file, named with the date and time, in the system's documents folder.
        /// </summary>
        public int Start()
        {
            return Start(CreateFileName(), -1, -1, -1);
        }

        /// <summary>
        /// Start recording the devices (webcam and/or audio input) played by the player to a file, named with the date and time, in the system's documents folder.
        /// </summary>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(int frameRate)
        {
            return Start(CreateFileName(), -1, -1, frameRate);
        }

        /// <summary>
        /// Start recording the devices (webcam and/or audio input) played by the player to a file, named with the date and time, in the system's documents folder.
        /// </summary>
        /// <param name="scale">The size of the video image in the recording as a percentage of the device's video image size. Values from 1 to 200 percent.</param>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(int scale, int frameRate)
        {
            return Start(CreateFileName(), scale, frameRate);
        }

        /// <summary>
        /// Start recording the devices (webcam and/or audio input) played by the player to a file, named with the date and time, in the system's documents folder.
        /// </summary>
        /// <param name="width">The width of the video image in the recording. A value of -1 represents the video image width of the device.</param>
        /// <param name="height">The height of the video image in the recording. A value of -1 represents the video image height of the device.</param>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(int width, int height, int frameRate)
        {
            return Start(CreateFileName(), width, height, frameRate);
        }

        /// <summary>
        /// Starts recording the devices (webcam and/or audio input) played by the player to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the recording. The file name extension can be changed by the recorder.</param>
        public int Start(string fileName)
        {
            return Start(fileName, -1, -1, -1);
        }

        /// <summary>
        /// Starts recording the devices (webcam and/or audio input) played by the player to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the recording. The file name extension can be changed by the recorder.</param>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(string fileName, int frameRate)
        {
            return Start(fileName, -1, -1, frameRate);
        }

        /// <summary>
        /// Starts recording the devices (webcam and/or audio input) played by the player to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the recording. The file name extension can be changed by the recorder.</param>
        /// <param name="scale">The size of the video image in the recording as a percentage of the device's video image size. Values from 1 to 200 percent.</param>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(string fileName, int scale, int frameRate)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                _base._lastError = HResult.MF_E_INVALIDREQUEST;
                if (!_base._recording)
                {
                    if (_base._webcamMode)
                    {
                        VideoTrack[] tracks = _base.AV_GetVideoTracks();

                        float rate = frameRate;
                        if (frameRate == -1) rate = tracks[0]._frameRate;

                        if (scale >= 1 && scale <= 200 && rate >= 1)
                        {
                            double factor = scale / 100.0;
                            _base._lastError = StartRecorder(fileName, (int)(tracks[0]._width * factor), (int)(tracks[0]._height * factor), rate);
                        }
                        else _base._lastError = HResult.E_INVALIDARG;
                    }
                    else if (_base._micMode)
                    {
                        _base._lastError = StartRecorder(fileName, 0, 0, 0);
                    }
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Starts recording the devices (webcam and/or audio input) played by the player to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the recording. The file name extension can be changed by the recorder.</param>
        /// <param name="width">The width of the video image in the recording. A value of -1 represents the video image width of the device.</param>
        /// <param name="height">The height of the video image in the recording. A value of -1 represents the video image height of the device.</param>
        /// <param name="frameRate">The video frame rate (fps) of the recording. A value of -1 represents the video frame rate of the device.</param>
        public int Start(string fileName, int width, int height, int frameRate)
        {
			if (!string.IsNullOrWhiteSpace(fileName))
			{
                _base._lastError = HResult.MF_E_INVALIDREQUEST;
                if (!_base._recording)
				{
					if (_base._webcamMode)
					{
						VideoTrack[] tracks = _base.AV_GetVideoTracks();
						if (width == -1) width = tracks[0]._width;
						if (height == -1) height = tracks[0]._height;

						float rate = frameRate;
						if (frameRate == -1) rate = tracks[0]._frameRate;

						if (width >= 8 && height >= 8 && rate >= 1) _base._lastError = StartRecorder(fileName, width, height, rate);
						else _base._lastError = HResult.E_INVALIDARG;
					}
                    else if (_base._micMode)
                    {
                        _base._lastError = StartRecorder(fileName, 0, 0, 0);
                    }
                }
			}
			else _base._lastError = HResult.ERROR_INVALID_NAME;
			return (int)_base._lastError;
        }

        /// <summary>
        /// Stops recording the devices (webcam and/or audio input) played by the player.
        /// </summary>
        public int Stop()
        {
            StopRecorder();

            _base._lastError = NO_ERROR;
            return NO_ERROR;
        }

        #endregion

        #region StartRecorder / StopRecorder

        // creates and starts the recorder
        private HResult StartRecorder(string fileName, int width, int height, float frameRate)
        {
            HResult         result          = NO_ERROR;

            IMFMediaSource  mf_AudioSource  = null;
            IMFMediaType    mf_AudioType    = null;

            IMFMediaSource  mf_VideoSource  = null;
            IMFMediaType    mf_VideoType    = null;

            IMFAttributes   attributes      = null;

            IMFCaptureSink  sink            = null;


            // create capture engine factory
            IMFCaptureEngineClassFactory factory = (IMFCaptureEngineClassFactory)new MFCaptureEngineClassFactory();
            if (factory != null)
            {
                // create capture engine
                result = factory.CreateInstance(MFAttributesClsid.CLSID_MFCaptureEngine, typeof(IMFCaptureEngine).GUID, out object engine);
                if (result == NO_ERROR)
                {
                    _recorder           = (IMFCaptureEngine)engine;
                    ErrorFromCallback   = NO_ERROR;
                    _hasAudio           = false;
                    _hasVideo           = false;

                    // create sources
                    if (_base._webcamMode)
                    {
                        _hasVideo = true;

                        // create video source
                        MFExtern.MFCreateAttributes(out IMFAttributes sourceAttr, 2);
                        sourceAttr.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
                        sourceAttr.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, _base._webcamDevice._id);

                        result = MFExtern.MFCreateDeviceSource(sourceAttr, out mf_VideoSource);
                        if (result == HResult.MF_E_ATTRIBUTENOTFOUND || result == HResult.ERROR_PATH_NOT_FOUND) result = HResult.MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED;
                        Marshal.ReleaseComObject(sourceAttr);

                        if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;

                        if (result == NO_ERROR)
                        {
                            if (!_base._webcamAggregated)
                            {
                                MFExtern.MFCreateAttributes(out attributes, 1);
                                attributes.SetUINT32(MFAttributesClsid.MF_CAPTURE_ENGINE_USE_VIDEO_DEVICE_ONLY, 1);
                            }
                            else
                            {
                                _hasAudio = true;

                                // create audio source
                                MFExtern.MFCreateAttributes(out sourceAttr, 2);
                                sourceAttr.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_GUID);
                                sourceAttr.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ENDPOINT_ID, _base._micDevice._id);
                                result = MFExtern.MFCreateDeviceSource(sourceAttr, out mf_AudioSource);
                                if (result == HResult.MF_E_ATTRIBUTENOTFOUND || result == HResult.ERROR_PATH_NOT_FOUND) result = HResult.MF_E_NO_AUDIO_RECORDING_DEVICE;
                                Marshal.ReleaseComObject(sourceAttr);

                                if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;
                            }
                        }
                    }
                    else // _micMode
                    {
                        _hasAudio = true;

                        // create audio source
                        MFExtern.MFCreateAttributes(out attributes, 1);
                        attributes.SetUINT32(MFAttributesClsid.MF_CAPTURE_ENGINE_USE_AUDIO_DEVICE_ONLY, 1);

                        MFExtern.MFCreateAttributes(out IMFAttributes sourceAttr, 2);
                        sourceAttr.SetGUID(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE, MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_GUID);
                        sourceAttr.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_ENDPOINT_ID, _base._micDevice._id);
                        result = MFExtern.MFCreateDeviceSource(sourceAttr, out mf_AudioSource);
                        if (result == HResult.MF_E_ATTRIBUTENOTFOUND || result == HResult.ERROR_PATH_NOT_FOUND) result = HResult.MF_E_NO_AUDIO_RECORDING_DEVICE;
                        Marshal.ReleaseComObject(sourceAttr);

                        if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;
                    }

                    if (result == NO_ERROR)
                    {
                        _callBack = new MF_RecorderCallBack(this);

                        AwaitCallBack = true;
                        result = _recorder.Initialize(_callBack, attributes, mf_AudioSource, mf_VideoSource);
                        _base.WaitForEvent.WaitOne(Player.TIMEOUT_30_SECONDS);

                        if (result == NO_ERROR) result = ErrorFromCallback;
                        if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;

                        if (result == NO_ERROR)
                        {
                            // get sink for recording audio and video
                            result = _recorder.GetSink(MF_CAPTURE_ENGINE_SINK_TYPE.Record, out sink);
                            if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;

                            if (result == NO_ERROR)
                            {
                                if (_hasVideo)
                                {
                                    mf_VideoType = CreateVideoMediaType(width, height, frameRate);
                                    result = ((IMFCaptureRecordSink)sink).AddStream(0, mf_VideoType, null, out _videoIndex);
                                    if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;
                                }

                                if (result == NO_ERROR)
                                {
                                    if (_hasAudio)
                                    {
                                        //AudioFormat oldFormat = _audioFormat;
                                        //bool changedFormat = false;
                                        if (_hasVideo && _audioFormat == AudioFormat.WMAudioV9 && _videoFormat != VideoFormat.WMV3)
                                        {
                                            _audioFormat = AudioFormat.AAC;
                                            //changedFormat = true;
                                        }
                                        AudioTrack[] tracks = _base.AV_GetAudioTracks();
                                        //mf_AudioType = CreateAudioMediaType(tracks[0]._sampleRate, tracks[0]._channelCount, (int)_audioBitRate);
                                        // TODO - always stereo 2 channels?
                                        mf_AudioType = CreateAudioMediaType(tracks[0]._sampleRate, 2, (int)_audioBitRate);
                                        if (mf_AudioType == null) result = HResult.MF_E_NOT_AVAILABLE;
                                        else
                                        {
                                            result = ((IMFCaptureRecordSink)sink).AddStream(_hasVideo ? 2 : 0, mf_AudioType, null, out _audioIndex);
                                            if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;
                                        }
                                        //if (changedFormat) _audioFormat = oldFormat;
                                    }

                                    if (result == NO_ERROR)
                                    {
                                        if (_setFileExtension)
                                        {
                                            if (_hasVideo)
                                            {
                                                switch (_videoFormat)
                                                {
                                                    case VideoFormat.WMV3:
                                                        fileName = Path.ChangeExtension(fileName, Player.WMV_FILE_EXTENSION);
                                                        break;

                                                    default: // VideoType.H264
                                                        fileName = Path.ChangeExtension(fileName, Player.MP4_FILE_EXTENSION);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                switch (_audioFormat)
                                                {
                                                    case AudioFormat.MP3:
                                                        fileName = Path.ChangeExtension(fileName, Player.MP3_FILE_EXTENSION);
                                                        break;

                                                    case AudioFormat.WMAudioV9:
                                                        fileName = Path.ChangeExtension(fileName, Player.WMA_FILE_EXTENSION);
                                                        break;

                                                    default: // AudioFormat.AAC:
                                                        fileName = Path.ChangeExtension(fileName, Player.AAC_FILE_EXTENSION);
                                                        break;
                                                }
                                            }
                                        }

                                        // set output file
                                        result = ((IMFCaptureRecordSink)sink).SetOutputFileName(fileName);
                                        if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;
                                    }
                                }
                            }
                        }
                    }
                }
                else result = HResult.MF_E_NOT_AVAILABLE;

                if (factory != null)        Marshal.ReleaseComObject(factory);

                if (sink != null)           Marshal.ReleaseComObject(sink);
                if (mf_AudioType != null)   Marshal.ReleaseComObject(mf_AudioType);
                if (mf_VideoType != null)   Marshal.ReleaseComObject(mf_VideoType);

                if (attributes != null)     Marshal.ReleaseComObject(attributes);

                if (mf_AudioSource != null) Marshal.ReleaseComObject(mf_AudioSource);
                if (mf_VideoSource != null) Marshal.ReleaseComObject(mf_VideoSource);

                if (result != NO_ERROR) StopRecorder();
                else
                {
                    AwaitCallBack = true;
                    result = _recorder.StartRecord();
                    _base.WaitForEvent.WaitOne(Player.TIMEOUT_30_SECONDS);

                    if (result == NO_ERROR) result = ErrorFromCallback;
                    if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;

                    if (result == NO_ERROR)
                    {
                        _base._deviceStart  = _base.PositionX;
                        _fileName           = fileName;
                        _base._recording    = true;

                        SetRecordingInfo();

                        _base._mediaDeviceRecorderStarted?.Invoke(_base, EventArgs.Empty);
                    }
                    else StopRecorder();
                }
            }
            if (result == HResult.MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED) result = NO_ERROR;

            _base._lastError = result;
            return result;
        }

        // stops and removes the recorder
        internal void StopRecorder()
        {
            if (_recorder != null)
            {
                if (_base._recording)
                {
                    if (!_resetAfterStop)
                    {
                        _base.AV_UpdateTopology();
                        if (_base._hasOverlay) _base.AV_ShowOverlay();
                    }

                    AwaitCallBack = true;
                    _recorder.StopRecord(true, false);
                    _base.WaitForEvent.WaitOne(Player.TIMEOUT_10_SECONDS);
                    _base._recording = false;

                    if(_setMetadata) AddMetadata();

                    _base._mediaDeviceRecorderStopped?.Invoke(this, EventArgs.Empty);

                    _base._deviceStart = _base.PositionX;
                }
                Marshal.ReleaseComObject(_recorder);
                _recorder = null;

                _fileName = string.Empty;
                _hasAudio = false;
                _hasVideo = false;

                _callBack.Dispose();
                _callBack = null;
            }
        }

        #endregion

        #region Create File Name / Create Media Types / AddMetadata / GetRecordingInfo

        private string CreateFileName()
        {
            string fileName = null;
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _folderName);
                string path = Path.Combine(folder, string.Format("Recordings {0:yyyy-MM-dd}", DateTime.Now));
                Directory.CreateDirectory(path);
                fileName = Path.Combine(path, string.Format("Recording {0:yyyy-MM-dd} at {0:HH-mm-ss}.mp4", DateTime.Now));
            }
            catch { /* ignored */ }
            return fileName;
        }

        private IMFMediaType CreateVideoMediaType(int width, int height, float frameRate)
        {
            const int VIDEO_BIT_RATE = 800000;

            MFExtern.MFCreateMediaType(out IMFMediaType mediaType);

            mediaType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);

            if (_videoFormat == VideoFormat.H264) mediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.H264);
            //else if (_videoType == VideoType.H265) mediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.H265);
            else mediaType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.WMV3);

            mediaType.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
            mediaType.SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, 2); // 2 = Progressive
            MFExtern.MFSetAttributeSize(mediaType, MFAttributesClsid.MF_MT_FRAME_SIZE, width, height);
            MFExtern.MFSetAttributeRatio(mediaType, MFAttributesClsid.MF_MT_FRAME_RATE, (int)(frameRate * 1000000), 1000000);
            MFExtern.MFSetAttributeRatio(mediaType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);

            return mediaType;
        }

        private IMFMediaType CreateAudioMediaType(int sampleRate, int numChannels, int bytesPerSecond)
        {
            IMFMediaType    mediaType = null;
            Guid            subType;

			switch (_audioFormat)
			{
				case AudioFormat.MP3:
					subType = MFMediaType.MP3;
					break;

                case AudioFormat.WMAudioV9:
                    subType = MFMediaType.WMAudioV9;
                    break;

                default: // AudioFormat.AAC:
					subType = MFMediaType.AAC;
					break;
			}

            if (MFExtern.MFTranscodeGetAudioOutputAvailableTypes(subType, MFT_EnumFlag.All, null, out IMFCollection availableTypes) == NO_ERROR)
            {
                availableTypes.GetElementCount(out int count);
                if (count > 0)
                {
                    object          element;
                    IMFMediaType    type;
					int             delta   = int.MaxValue;
					int             index   = -1;

                    for (int i = 0; i < count; i++)
                    {
                        availableTypes.GetElement(i, out element);
                        type = (IMFMediaType)element;

                        type.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_SAMPLES_PER_SECOND, out int typeSampleRate);
                        if (typeSampleRate == sampleRate)
                        {
							type.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_NUM_CHANNELS, out int typeNumChannels);
							if (typeNumChannels == numChannels)
							{
								type.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_AVG_BYTES_PER_SECOND, out int typeBytesPerSecond);
								int diff = Math.Abs(typeBytesPerSecond - bytesPerSecond);
								if (diff < delta)
								{
									index = i;
									delta = diff;
									if (diff == 0) break;
								}
							}
						}
                        Marshal.ReleaseComObject(element);
                    }
                    availableTypes.GetElement(index == -1 ? 0 : index, out element);
                    mediaType = (IMFMediaType)element;
                }
                Marshal.ReleaseComObject(availableTypes);
            }
            return mediaType;
        }

        private void AddMetadata()
        {
            Guid iid                = typeof(IPropertyStore).GUID;
            IPropertyStore store    = null;

            if (!string.IsNullOrWhiteSpace(_fileName))
            {
                try
                {
                    if (MFExtern.SHGetPropertyStoreFromParsingName(_info._fileName, IntPtr.Zero, GETPROPERTYSTOREFLAGS.GPS_READWRITE, ref iid, out store) == NO_ERROR)
                    {
                        PropVariant prop = new PropVariant(string.Format(Path.GetFileNameWithoutExtension(_fileName)));
                        store.SetValue(PropertyKeys.PKEY_Title, prop);
                        prop.Dispose();

                        if (_info._hasVideo)
                        {
                            prop = new PropVariant(_info._videoDeviceName);
                            store.SetValue(PropertyKeys.PKEY_Media_SubTitle, prop);
                            prop.Dispose();
                        }
                        else if (_info._hasAudio)
                        {
                            prop = new PropVariant(_info._audioDeviceName);
                            store.SetValue(PropertyKeys.PKEY_Media_SubTitle, prop);
                            prop.Dispose();
                        }

                        prop = new PropVariant(_info._recorderName);
                        store.SetValue(PropertyKeys.PKEY_Music_Artist, prop);
                        prop.Dispose();

                        prop = new PropVariant("");
                        store.SetValue(PropertyKeys.PKEY_Comment, prop);
                        prop.Dispose();

                        prop = new PropVariant(DateTime.Now.Year);
						store.SetValue(PropertyKeys.PKEY_Media_Year, prop);
                        prop.Dispose();

                        store.Commit();
					}
				}
                catch {  /* ignored */ }
                if (store != null) Marshal.ReleaseComObject(store);
            }
        }

        private void SetRecordingInfo()
        {
            if (_base._recording)
            {
                if (_recorder.GetSink(MF_CAPTURE_ENGINE_SINK_TYPE.Record, out IMFCaptureSink sink) == NO_ERROR)
                {
                    _info._audio    = null;
                    _info._video    = null;

                    _info = new Recording
                    {
                        _recorderName   = _base.Name,
						_fileName       = _fileName,

						_hasAudio       = _hasAudio,
						_audio          = new AudioTrack(),

						_hasVideo       = _hasVideo,
						_video          = new VideoTrack()
					};

					if (_hasVideo)
                    {
                        _info._videoDeviceName = _base._webcamDevice._name;

                        sink.GetOutputMediaType(_videoIndex, out IMFMediaType videoType);
                        videoType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out _info._video._mediaType);
                        MFExtern.MFGetAttributeRatio(videoType, MFAttributesClsid.MF_MT_FRAME_RATE, out int num, out int denum);
                        if (denum > 0) _info._video._frameRate = (float)(uint)num / denum;
                        MFExtern.MFGetAttributeRatio(videoType, MFAttributesClsid.MF_MT_FRAME_SIZE, out _info._video._width, out _info._video._height);
                        Marshal.ReleaseComObject(videoType);
                    }

                    if (_hasAudio)
                    {
                        _info._audioDeviceName = _base._micDevice.ToString();

                        sink.GetOutputMediaType(_audioIndex, out IMFMediaType audioType);
                        audioType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out _info._audio._mediaType);
                        audioType.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_NUM_CHANNELS, out _info._audio._channelCount);
                        audioType.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_SAMPLES_PER_SECOND, out _info._audio._sampleRate);
                        audioType.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_BITS_PER_SAMPLE, out _info._audio._bitDepth);
                        audioType.GetUINT32(MFAttributesClsid.MF_MT_AUDIO_AVG_BYTES_PER_SECOND, out _info._audio._bitRate);
                        _info._audio._bitRate *= 8;
                        Marshal.ReleaseComObject(audioType);
                    }

                    Marshal.ReleaseComObject(sink);
                }
            }
        }

        #endregion

    }

    #endregion

    #region Display Class

    /// <summary>
    /// A class that is used to group together the Display methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Display : HideObjectMembers
    {
        #region Fields (Display Class)

        private const int   NO_ERROR        = 0;

        private Player      _base;

		private bool        _isDragging;

		private Cursor      _oldCursor;
		private Cursor      _dragCursor     = Cursors.SizeAll;

		private bool        _setDragCursor  = true;

		private Point       _oldLocation;

		private Form        _dragForm;

        #endregion

        internal Display(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets the player's display window (form or control) that is used to display video and overlays.
        /// </summary>
        public Control Window
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._display;
            }
            set { _base.AV_SetDisplay(value, true); }
        }

        /// <summary>
        /// Gets or sets the display mode (size and location) of the video image on the player's display window (default: DisplayMode.ZoomCenter). See also: Player.Video.Bounds.
        /// </summary>
        public DisplayMode Mode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._displayMode;
            }
            set
            {
                _base._lastError = NO_ERROR;

                if (_base._displayMode != value)
                {
                    _base._displayMode = value;
                    if (value == DisplayMode.Manual)
                    {
                        if (!_base._hasVideoBounds)
                        {
                            _base._videoBounds.X = _base._videoBounds.Y = 0;
                            _base._videoBounds.Size = _base._display.Size;
                            _base._videoBoundsClip = _base._videoBounds;
                            _base._hasVideoBounds = true;
                        }
                    }
                    else _base._hasVideoBounds = false;

                    if (_base._hasDisplay) _base._display.Invalidate();
                    if (_base._hasDisplayShape) _base.AV_UpdateDisplayShape();
                    if (_base.dc_DisplayClonesRunning) _base.DisplayClones_Refresh();
                    _base._mediaDisplayModeChanged?.Invoke(_base, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Provides access to the display overlay settings of the player (for example, Player.Display.Overlay.Window).
        /// </summary>
        public Overlay Overlay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.Overlay;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player's full screen mode is activated (default: false). See also: Player.Display.FullScreenMode.
        /// </summary>
        public bool FullScreen
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._fullScreen;
            }
            set { _base.AV_SetFullScreen(value); }
        }

        /// <summary>
        /// Gets or sets the player's full screen display mode (default: FullScreenMode.Display). See also: Player.Display.FullScreen.
        /// </summary>
        public FullScreenMode FullScreenMode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._fullScreenMode;
            }
            set { _base.FullScreenMode = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player's full screen display mode on all screens (video wall) is activated (default: false). See also: Player.Display.FullScreen and Player.Display.FullScreenMode.
        /// </summary>
        public bool Wall
        {
            get { return _base.FS_GetVideoWallMode(); }
            set { _base.FS_SetVideoWallMode(value); }
        }

        /// <summary>
        /// Gets the size and location of the parent window (form) of the player's display window in its normal window state.
        /// </summary>
        public Rectangle RestoreBounds
        {
            get
            {
                Rectangle r = Rectangle.Empty;

                _base._lastError = NO_ERROR;
                if (_base._fullScreen)
                {
                    r = _base._fsFormBounds;
                }
                else
                {
                    if (_base._hasDisplay)
                    {
                        Form f = _base._display.FindForm();
                        r = f.WindowState == FormWindowState.Normal ? f.Bounds : f.RestoreBounds;
                    }
                    else
                    {
                        _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    }
                }
                return r;
            }
        }

        /// <summary>
        /// Gets or sets the shape of the player's display window. See also: Player.Display.GetShape and Player.Display.SetShape.
        /// </summary>
        public DisplayShape Shape
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._displayShape;
            }
            set
            {
                SetShape(value, _base._hasVideoShape, _base._hasOverlayClipping);
            }
        }

        /// <summary>
        /// Gets the shape of the player's display window (default: DisplayShape.Normal). See also: Player.Display.Shape.
        /// </summary>
        /// <param name="shape">A value that indicates the shape of the player's display window.</param>
        /// <param name="videoShape">A value that indicates whether the shape applies to the video image (or to the display window).</param>
        /// <param name="overlayShape">A value that indicates whether the shape is also applied to display overlays.</param>
        public int GetShape(out DisplayShape shape, out bool videoShape, out bool overlayShape)
        {
            _base._lastError = NO_ERROR;

            shape           = _base._displayShape;
            videoShape      = _base._hasVideoShape;
            overlayShape    = _base._hasOverlayClipping;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Sets the shape of the player's display window. See also: Player.Display.Shape.
        /// </summary>
        /// <param name="shape">A value that indicates the shape of the player's display window.</param>
        public int SetShape(DisplayShape shape)
        {
            return SetShape(shape, _base._hasVideoShape, _base._hasOverlayClipping);
        }

        /// <summary>
        /// Sets the shape of the player's display window. See also: Player.Display.Shape.
        /// </summary>
        /// <param name="shape">A value that indicates the shape of the player's display window.</param>
        /// <param name="videoShape">A value that indicates whether the shape applies to the video image (or to the display window).</param>
        /// <param name="overlayShape">A value that indicates whether the shape should also be applied to display overlays.</param>
        public int SetShape(DisplayShape shape, bool videoShape, bool overlayShape)
        {
            _base._lastError = NO_ERROR;

            if (shape == DisplayShape.Normal)
            {
                _base.AV_RemoveDisplayShape(true);
            }
            else
            {
                if (_base._hasDisplay)
                {
                    if (_base._displayShape != shape || videoShape != _base._hasVideoShape || overlayShape != _base._hasOverlayClipping)
                    {
                        _base._displayShape         = shape;
                        _base._hasVideoShape        = videoShape;
                        _base._displayShapeCallback = _base.AV_GetShapeCallback(shape);
                        _base.AV_SetOverlayClipping(overlayShape);

                        _base._hasDisplayShape = true;
                        _base.AV_UpdateDisplayShape();

                        if (videoShape)
                        {
                            _base._mediaVideoBoundsChanged += _base.AV_DisplayShapeChanged;
                        }
                        _base._mediaDisplayShapeChanged?.Invoke(_base, EventArgs.Empty);
                    }
                }
                else
                {
                    if (_base._displayShape != DisplayShape.Normal) _base.AV_RemoveDisplayShape(true);
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE; // No Display
                }
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets a custom display shape. The shape can be activated with Player.Display.Shape = DisplayShape.Custom.
        /// </summary>
        public GraphicsPath CustomShape
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._customShapePath;
            }
            set
            {
                if (value == null)
                {
                    _base._customShapePath = null;
                    if (_base._hasDisplayShape && _base._displayShape == DisplayShape.Custom)
                    {
                        _base.AV_RemoveDisplayShape(true);
                    }
                }
                else
                {
                    _base._customShapePath = (GraphicsPath)value.Clone();
                    if (_base._hasDisplayShape && _base._displayShape == DisplayShape.Custom)
                    {
                        _base.AV_UpdateDisplayShape();
                    }
                }
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets the cursor that is used when the player's display window is dragged (default: Cursors.SizeAll). See also: Player.Display.DragEnabled.
        /// </summary>
        public Cursor DragCursor
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _dragCursor;
            }
            set
            {
                _dragCursor = value;
                _setDragCursor = value != Cursors.Default;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the parent window of the player's display window can be moved by dragging the player's display window (default: false). See also: Player.Display.DragCursor.
        /// </summary>
        public bool DragEnabled
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._dragEnabled;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value)
                {
                    if (!_base._dragEnabled)
                    {
                        if (!_base._hasDisplay || _base._display.FindForm() == null)
                        {
                            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                        }
                        else
                        {
                            _base._display.MouseDown += Drag_MouseDown;
                            _base._dragEnabled = true;
                        }
                    }
                }
                else if (_base._dragEnabled)
                {
                    _base._display.MouseDown -= Drag_MouseDown;
                    _base._dragEnabled = false;
                }
            }
        }

        /// <summary>
        /// Drags the parent window (form) of the player's display window. Use as the mousedown eventhandler of any control other than the player's display window (see Player.Display.DragEnabled), for example, a display overlay. 
        /// </summary>
        public void Drag_MouseDown(object sender, MouseEventArgs e)
		{
			if (!_isDragging && e.Button == MouseButtons.Left)
			{
				if (sender != null && _base._hasDisplay && !_base._fullScreen)
				{
					_dragForm = _base._display.FindForm();
					if (_base._hasOverlay)
					{
						foreach (Form f in Application.OpenForms)
						{
							if (f != _base._overlay && f.Owner == _base._overlay.Owner) f.BringToFront();
						}
					}
					_dragForm.Activate();

					if (_dragForm.WindowState != FormWindowState.Maximized)
					{
						Control control = (Control)sender;

						_oldLocation = control.PointToScreen(e.Location);

						control.MouseMove += DragDisplay_MouseMove;
						control.MouseUp += DragDisplay_MouseUp;

						if (_setDragCursor)
						{
							_oldCursor = control.Cursor;
							control.Cursor = _dragCursor;
						}

						_isDragging = true;
					}
					else
					{
						_dragForm = null;
					}
				}
			}
		}

		private void DragDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point location  = ((Control)sender).PointToScreen(e.Location);

                _dragForm.Left  += location.X - _oldLocation.X;
                _dragForm.Top   += location.Y - _oldLocation.Y;
                _oldLocation    = location;
            }
        }

        private void DragDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Control control     = (Control)sender;

                control.MouseMove   -= DragDisplay_MouseMove;
                control.MouseUp     -= DragDisplay_MouseUp;
                _dragForm           = null;

                if (_setDragCursor) control.Cursor = _oldCursor;

                _isDragging         = false;
            }
        }

        /// <summary>
        /// Updates the video image on the player's display window. For special use only, generally not required.
        /// </summary>
        public int Update()
        {
            _base._lastError = NO_ERROR;
            if (_base._webcamMode)
            {
                _base.AV_UpdateTopology();
                if (_base._hasOverlay) _base.AV_ShowOverlay();
            }
            else if (_base.mf_VideoDisplayControl != null)
            {
                _base.mf_VideoDisplayControl.RepaintVideo();
                if (_base._display != null) _base._display.Invalidate();
            }
            else
            {
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the contents of the player's display window are retained after media has finished playing (default: false). Can be used to smooth the transition between media. If set to true, the value must be reset to false when all media playback is complete to clear the display. See also: Player.Display.HoldClear.
        /// </summary>
        public bool Hold
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._displayHold;
            }
            set
            {
                if (value != _base._displayHold)
                {
                    _base._displayHold = value;
                    if (!value) _base.AV_ClearHold();
                }
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Clears the player's display window when the Player.Display.Hold option is enabled and no media is playing. See also: Player.Display.Hold.
        /// </summary>
        public int HoldClear()
        {
            if (_base._displayHold)
            {
                _base.AV_ClearHold();
                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.MF_E_INVALIDREQUEST;
            return (int)_base._lastError;
        }
    }

    #endregion

    #region CursorHide Class

    /// <summary>
    /// A class that is used to group together the CursorHide methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CursorHide : HideObjectMembers
    {
        #region Fields (CursorHide Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal CursorHide(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Adds the specified form to the forms that automatically hide the cursor (mouse pointer) during inactivity when media is playing.
        /// </summary>
        /// <param name="form">The form to add.</param>
        public int Add(Form form)
        {
            _base._lastError = Player.CH_AddItem(form, _base); //, _base._display);
            _base._hasCursorHide = Player.CH_HasItems(_base);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes the specified form from the forms that automatically hide the cursor.
        /// </summary>
        /// <param name="form">The form to remove.</param>
        public int Remove(Form form)
        {
            _base._lastError = Player.CH_RemoveItem(form, _base); //, _base._display);
            _base._hasCursorHide = Player.CH_HasItems(_base);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes all forms added by this player from the forms that automatically hide the cursor.
        /// </summary>
        public int RemoveAll()
        {
            Player.CH_RemovePlayerItems(_base);
            _base._hasCursorHide = false;
            _base._lastError = NO_ERROR;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether automatic hiding of the cursor is enabled. This option can be used to temporarily disable the hiding of the cursor. This setting is used by all players in this assembly.
        /// </summary>
        public bool Enabled
        {
            get
            {
                _base._lastError = NO_ERROR;
                return !Player.ch_Disabled;
            }
            set
            {
                //Player.ch_EventArgs._reason = CursorChangedReason.UserCommand;
                Player.CH_Disabled = !value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds to wait before the cursor is hidden during inactivity when media is playing. This setting is used by all players in this assembly (default: 3 seconds).
        /// </summary>
        public int Delay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return Player.ch_Delay;
            }
            set
            {
                if (value < 1) value = 1;
                else if (value > 30) value = 30;
                if (value != Player.ch_Delay)
                {
                    Player.ch_Delay = value;
                    Player.ch_Timer.Interval = value == 1 ? 500 : 1000; // value  * 500;
                }
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Get a value that indicates whether the cursor has been hidden by the player.
        /// </summary>
        public bool CursorHidden
        {
            get
            {
                _base._lastError = NO_ERROR;
                return Player.ch_Hidden;
            }
            set
            {
                if (value) { Player.CH_HideCursor(); }
                else { Player.CH_ShowCursor(); }

                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Shows the cursor if it was hidden by the player. The cursor is also shown again when the mouse device is moved.
        /// </summary>
        public int ShowCursor()
        {
            Player.CH_ShowCursor();
            _base._lastError = NO_ERROR;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Hides the cursor if the CursorHide option is enabled.
        /// </summary>
        public int HideCursor()
        {
            Player.CH_HideCursor();
            _base._lastError = NO_ERROR;
            return (int)_base._lastError;
        }
    }

    #endregion

    #region Overlay Class

    /// <summary>
    /// A class that is used to group together the Display Overlay methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Overlay : HideObjectMembers
    {
        #region Fields (Overlay Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Overlay(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets the player's display overlay.
        /// </summary>
        public Form Window
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._overlay;
            }
            set { _base.AV_SetOverlay(value); }
        }

        /// <summary>
        /// Sets the specified form as the player's display overlay. Same as Player.Overlay.Window = form.
        /// </summary>
        /// <param name="form">The form to be set as the player's display overlay.</param>
        public void Set(Form form)
        {
            _base.AV_SetOverlay(form);
        }

        /// <summary>
        /// Removes the display overlay from the player. Same as Player.Overlay.Window = null.
        /// </summary>
        public void Remove()
        {
            _base.AV_RemoveOverlay(true);
        }

        /// <summary>
        /// Gets or sets the display mode (video or display size) of the player's display overlay (default: OverlayMode.Video).
        /// </summary>
        public OverlayMode Mode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._overlayMode;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base._overlayMode)
                {
                    _base._overlayMode = value;
                    if (_base._hasDisplay && _base._hasOverlayShown)
                    {
                        _base._display.Invalidate();
                        if (_base._hasOverlayClipping) _base.AV_ClipOverlay();
                    }
                    _base._mediaOverlayModeChanged?.Invoke(_base, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player's display overlay is always shown (default: false).
        /// </summary>
        public bool Hold
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._overlayHold;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base._overlayHold)
                {
                    _base._overlayHold = value;
                    if (_base._hasOverlay)
                    {
                        if (value)
                        {
                            if (_base._hasDisplay && !_base._hasOverlayShown && _base._display.FindForm().Visible)
                            {
                                _base.AV_ShowOverlay();
                                if (_base.dc_HasDisplayClones) _base.DisplayClones_Start();
                            }
                        }
                        else if (_base._hasOverlayShown && (!_base._playing))
                        {
                            bool tempHold = _base._displayHold;
                            _base._displayHold = false;
                            _base.AV_HideOverlay();
                            //if (_base.dc_HasDisplayClones && !_base._playing) _base.DisplayClones_Stop(false);
                            _base._displayHold = tempHold;
                        }
                    }
                    _base._mediaOverlayHoldChanged?.Invoke(_base, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player's display overlay can be activated for input and selection (default: false).
        /// </summary>
        public bool CanFocus
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._overlayCanFocus;
            }
            set
            {
                if (value != _base._overlayCanFocus) _base.AV_SetOverlayCanFocus(value);
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds that the visibilty of the player's display overlay is delayed when restoring the player's minimized display window (form). Set the value to 0 to disable (default: 200 ms).
        /// </summary>
        public int Delay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._minimizedInterval;
            }
            set
            {
                if (value == 0)
                {
                    _base._minimizedInterval = 0;
                    _base._minimizeEnabled = false;
                    _base.AV_MinimizeActivate(false);
                }
                else
                {
                    if (value < 100) value = 100;
                    else if (value > 1500) value = 1500;
                    _base._minimizedInterval = value;
                    _base._minimizeEnabled = true;
                    _base.AV_MinimizeActivate(true);
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player's display overlay is active. See also: Player.Overlay.Present and Player.Overlay.Visible.
        /// </summary>
        public bool Active
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlayShown;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has a display overlay (set, but not necessarily active or visible). See also: Player.Overlay.Active and Player.Overlay.Visible.
        /// </summary>
        public bool Present
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlay;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player's display overlay is active and visible. See also: Player.Overlay.Active and Player.Overlay.Present.
        /// </summary>
        public bool Visible
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlay && _base._overlay.Visible;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether clipping of the player's display overlay is enabled. The overlay is clipped when it protrudes outside the parent form of the player's display window (default: false).
        /// </summary>
        public bool Clipping
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlayClipping;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base._hasOverlayClipping)
                {
                    _base.AV_SetOverlayClipping(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the opacity of display overlays displayed on screenshots and display clones (default: OverlayBlend.None).
        /// </summary>
        public OverlayBlend Blend
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._overlayBlend;
            }
            set
            {
                _base._lastError = NO_ERROR;
                _base._blendFunction.AlphaFormat = value == OverlayBlend.Transparent ? SafeNativeMethods.AC_SRC_ALPHA : SafeNativeMethods.AC_SRC_OVER;
                _base._overlayBlend = value;
            }
        }

        /// <summary>
        /// Gets the size and location (in pixels) of the display overlay window relative to the player's display window.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                Rectangle bounds;

                _base._lastError = NO_ERROR;
                if (_base._hasOverlayShown)
                {
                    if (_base._hasVideo && _base._overlayMode == OverlayMode.Video) bounds = _base._videoBounds;
                    else bounds = new Rectangle(Point.Empty, _base._display.Size);
                }
                else bounds = Rectangle.Empty;

                return bounds;
            }
        }

    }

    #endregion

    #region DisplayClones Class

    /// <summary>
    /// A class that is used to group together the Display Clones methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DisplayClones : HideObjectMembers
    {
        #region Fields (DisplayClones Class)

        private const int       NO_ERROR        = 0;
        //private const CloneFlip     DEFAULT_FLIP    = CloneFlip.FlipNone;
        //private const CloneLayout   DEFAULT_LAYOUT  = CloneLayout.Zoom;
        //private const CloneQuality  DEFAULT_QUALITY = CloneQuality.Auto;
        private const int       MAX_FRAMERATE   = 100;

        private Player          _base;
        private CloneProperties _defaultProps;

        #endregion

        internal DisplayClones(Player player)
        {
            _base = player;
            _defaultProps = new CloneProperties();
        }

        /// <summary>
        /// Adds the specified control as a display clone to the player.
        /// </summary>
        /// <param name="clone">The form or control to add as a display clone.</param>
        public int Add(Control clone)
        {
            return (int)_base.DisplayClones_Add(new Control[] { clone }, _defaultProps);
        }

        /// <summary>
        /// Adds the specified control as a display clone to the player.
        /// </summary>
        /// <param name="clone">The control to add as a display clone.</param>
        /// <param name="properties">The properties to be applied to the display clone.</param>
        public int Add(Control clone, CloneProperties properties)
        {
            _base._lastError = HResult.E_INVALIDARG;

            if (clone != null)
            {
                _base.DisplayClones_Add(new Control[] { clone }, properties);
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Adds the specified controls as display clones to the player.
        /// </summary>
        /// <param name="clones">The controls to add as display clones.</param>
        public int AddRange(Control[] clones)
        {
            return (int)_base.DisplayClones_Add(clones, _defaultProps);
        }

        /// <summary>
        /// Adds the specified controls as display clones to the player.
        /// </summary>
        /// <param name="clones">The controls to add as display clones.</param>
        /// <param name="properties">The properties to be applied to the display clones.</param>
        public int AddRange(Control[] clones, CloneProperties properties)
        {
            _base.DisplayClones_Add(clones, properties);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes the specified display clone from the player.
        /// </summary>
        /// <param name="clone">The display clone to remove from the player.</param>
        public int Remove(Control clone)
        {
            _base._lastError = NO_ERROR;

            if (clone != null)
            {
                _base.DisplayClones_Remove(new Control[] { clone });
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes the specified display clones from the player.
        /// </summary>
        /// <param name="clones">The display clones to remove from the player.</param>
        public int RemoveRange(Control[] clones)
        {
            _base._lastError = NO_ERROR;

            if (clones != null)
            {
                _base.DisplayClones_Remove(clones);
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes all display clones from the player.
        /// </summary>
        public int RemoveAll()
        {
            return (int)_base.DisplayClones_Clear();
        }

        /// <summary>
        /// Gets the number of display clones of the player.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                _base._lastError = NO_ERROR;

                if (_base.dc_HasDisplayClones)
                {
                    for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                    {
                        if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control != null) count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the specified control is a display clone of the player.
        /// </summary>
        /// <param name="control">The control to search for.</param>
        public bool Contains(Control control)
        {
            bool found = false;
            _base._lastError = NO_ERROR;

            if (_base.dc_HasDisplayClones && control != null)
            {
                for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                {
                    if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control == control)
                    {
                        found = true;
                        break;
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Returns an array of the player's display clones.
        /// </summary>
        public Control[] GetClones()
        {
            Control[] items = null;
            _base._lastError = NO_ERROR;

            if (_base.dc_HasDisplayClones)
            {
                int count = 0;

                for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                {
                    if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control != null) count++;
                }

                if (count > 0)
                {
                    int index = 0;
                    items = new Control[count];

                    for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                    {
                        if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control != null) items[index++] = _base.dc_DisplayClones[i].Control;
                    }
                }
            }
            return items;
        }

        /// <summary>
        /// Gets or sets a value that indicates the number of video frames per second used for displaying the player's display clones (default: 30 fps).
        /// </summary>
        public int FrameRate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.dc_CloneFrameRate;
            }
            set
            {
                _base._lastError = NO_ERROR;

                if (value <= 1) value = 1;
                else if (value > MAX_FRAMERATE) value = MAX_FRAMERATE;
                _base.dc_CloneFrameRate = value;
                _base.dc_TimerInterval = 1000 / value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player's display clones also show display overlays (default: true).
        /// </summary>
        public bool ShowOverlay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.dc_CloneOverlayShow;
            }
            set
            {
                _base._lastError = NO_ERROR;
                _base.dc_CloneOverlayShow = value;

                if (_base.dc_HasDisplayClones)
                {
                    if (value)
                    {
                        if (!_base.dc_DisplayClonesRunning && _base._hasOverlay && _base._overlayHold)
                        {
                            _base.DisplayClones_Start();
                        }
                    }
                    else if (_base.dc_DisplayClonesRunning)
                    {
                        if (!_base._playing)
                        {
                            _base.DisplayClones_Stop(false);
                        }
                        else if (!_base._hasVideo) // invalidate clone display
                        {
                            for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                            {
                                if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control != null)
                                {
                                    _base.dc_DisplayClones[i].Control.Invalidate();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the adjustable properties of the specified display clone of the player.
        /// </summary>
        /// <param name="clone">The display clone whose properties are to be obtained.</param>
        public CloneProperties GetProperties(Control clone)
        {
            CloneProperties properties = null;

            int index = GetCloneIndex(clone);
            if (index != -1)
            {
                properties = new CloneProperties
                {
                    _dragEnabled    = _base.dc_DisplayClones[index].Drag,
                    _flip           = _base.dc_DisplayClones[index].Flip,
                    _layout         = _base.dc_DisplayClones[index].Layout,
                    _quality        = _base.dc_DisplayClones[index].Quality
                };

                if (_base.dc_DisplayClones[index].HasShape)
                {
                    properties._shape       = _base.dc_DisplayClones[index].Shape;
                    properties._videoShape  = _base.dc_DisplayClones[index].HasVideoShape;
                }
                _base._lastError = NO_ERROR;
            }
            return properties;
        }

        /// <summary>
        /// Sets the specified properties to the specified display clone of the player.
        /// </summary>
        /// <param name="clone">The display clone whose properties are to be set.</param>
        /// <param name="properties">The properties to be set.</param>
        public int SetProperties(Control clone, CloneProperties properties)
        {
            int index = GetCloneIndex(clone);
            if (index != -1)
            {
                SetCloneProperties(_base.dc_DisplayClones[index], properties);
                _base._lastError = NO_ERROR;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Sets the specified properties to all display clones of the player.
        /// </summary>
        /// <param name="properties">The properties to be set.</param>
        public int SetProperties(CloneProperties properties)
        {
            _base._lastError = NO_ERROR;

            if (_base.dc_HasDisplayClones)
            {
                for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                {
                    if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control != null)
                    {
                        SetCloneProperties(_base.dc_DisplayClones[i], properties);
                    }
                }
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Returns the size and location of the video image of the specified display clone of the player.
        /// </summary>
        /// <param name="clone">The display clone whose video bounds has to be obtained.</param>
        public Rectangle GetVideoBounds(Control clone)
        {
            Rectangle bounds = Rectangle.Empty;

            int index = GetCloneIndex(clone);
            if (index != -1)
            {
                if ((!_base._hasVideo && !(_base._hasOverlay && _base._overlayHold)) || _base._displayMode == DisplayMode.Stretch || _base.dc_DisplayClones[index].Layout == CloneLayout.Stretch || _base.dc_DisplayClones[index].Layout == CloneLayout.Cover)// || (_base._hasOverlay && _base._overlayMode == OverlayMode.Display))
                {
                    bounds = _base.dc_DisplayClones[index].Control.DisplayRectangle;
                }
                else
                {
                    int newSize;
                    Rectangle sourceRect = _base._videoBoundsClip;
                    Rectangle destRect  = _base.dc_DisplayClones[index].Control.DisplayRectangle;

                    double difX         = (double)destRect.Width / sourceRect.Width;
                    double difY         = (double)destRect.Height / sourceRect.Height;

                    if (difX < difY)
                    {
                        newSize         = (int)(sourceRect.Height * difX);

                        bounds.X        = 0;
                        bounds.Y        = (destRect.Height - newSize) / 2;
                        bounds.Width    = (int)(sourceRect.Width * difX);
                        bounds.Height   = newSize;
                    }
                    else
                    {
                        newSize         = (int)(sourceRect.Width * difY);

                        bounds.X        = (destRect.Width - newSize) / 2;
                        bounds.Y        = 0;
                        bounds.Width    = newSize;
                        bounds.Height   = (int)(sourceRect.Height * difY);
                    }
                }
            }
            return bounds;
        }

        private void SetCloneProperties(Player.Clone clone, CloneProperties properties)
        {
            if (clone.Drag != properties._dragEnabled)
            {
                if (properties._dragEnabled)
                {
                    clone.Control.MouseDown += _base.DisplayClones_MouseDown;
                    clone.Drag = true;
                }
                else
                {
                    clone.Control.MouseDown -= _base.DisplayClones_MouseDown;
                    clone.Drag = false;
                }
            }
            clone.DragCursor = properties._dragCursor;
            clone.Flip = properties._flip;
            clone.Layout = properties._layout;
            clone.Quality = properties._quality;
            if (clone.Shape != properties._shape || clone.HasVideoShape != properties._videoShape)
            {
                SetCloneShape(clone, properties._shape, properties._videoShape);
            }

            clone.Refresh = true;
        }

        private void SetCloneShape(Player.Clone clone, DisplayShape shape, bool videoShape)
        {
            Region oldRegion = null;

            try
            {
                bool set = shape != DisplayShape.Normal;
                if (clone.HasShape)
                {
                    if (clone.Shape == shape) set = false;
                    else
                    {
                        clone.HasShape = false;
                        clone.ShapeCallback = null;
                        clone.Shape = DisplayShape.Normal;

                        if (!set && clone.Control.Region != null)
                        {
                            clone.Control.Region.Dispose();
                            clone.Control.Region = null;
                            if (_base._paused) clone.Control.Invalidate();
                        }
                        else oldRegion = clone.Control.Region;
                    }
                }
                if (set)
                {
                    clone.Shape = shape;
                    clone.HasVideoShape = videoShape;
                    clone.ShapeCallback = _base.AV_GetShapeCallback(shape);
                    clone.HasShape = true;

                    if (oldRegion != null) oldRegion.Dispose();
                    //while (_base.dc_PaintBusy)
                    //{
                    //    System.Threading.Thread.Sleep(1);
                    //    Application.DoEvents();
                    //}
                    clone.Control.Invalidate();
                }
            }
            catch { /* ignored */ }
        }

        private int GetCloneIndex(Control clone)
        {
            int index = -1;
            if (_base.dc_HasDisplayClones && clone != null)
            {
                for (int i = 0; i < _base.dc_DisplayClones.Length; i++)
                {
                    if (_base.dc_DisplayClones[i] != null && _base.dc_DisplayClones[i].Control == clone)
                    {
                        index = i;
                        break;
                    }
                }
            }
            _base._lastError = index == -1 ? HResult.E_INVALIDARG : NO_ERROR;
            return index;
        }
    }

    #endregion

    #region PointTo Class

    /// <summary>
    /// A class that is used to group together the Point To conversion methods of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class PointTo : HideObjectMembers
    {
        #region Fields (PointTo Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal PointTo(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Converts the specified screen location to coordinates of the player's display window.
        /// </summary>
        /// <param name="p">The screen coordinate to convert.</param>
        public Point Display(Point p)
        {
            if (_base._hasDisplay && _base._display.Visible)
            {
                _base._lastError = NO_ERROR;
                return _base._display.PointToClient(p);
            }
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return new Point(-1, -1);
        }

        /// <summary>
        /// Converts the specified screen location to coordinates of the player's display overlay.
        /// </summary>
        /// <param name="p">The screen coordinate to convert.</param>
        public Point Overlay(Point p)
        {
            if (_base._hasOverlay && _base._overlay.Visible)
            {
                _base._lastError = NO_ERROR;
                return _base._overlay.PointToClient(p);
            }
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return new Point(-1, -1);
        }

        /// <summary>
        /// Converts the specified screen location to coordinates of the video image on the player's display window.
        /// </summary>
        /// <param name="p">The screen coordinate to convert.</param>
        public Point Video(Point p)
        {
            Point newP = new Point(-1, -1);
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;

            if (_base._hasVideo)
            {
                Point p2 = _base._display.PointToClient(p);
                if (_base._videoBoundsClip.Contains(p2))
                {
                    newP.X = p2.X - _base._videoBounds.X;
                    newP.Y = p2.Y - _base._videoBounds.Y;
                    _base._lastError = NO_ERROR;
                }
            }
            return newP;
        }

#pragma warning disable CA1822 // Mark members as static

        /// <summary>
        /// Returns the slider value at the specified location on the specified slider (trackbar).
        /// </summary>
        /// <param name="slider">The slider whose value should be obtained.</param>
        /// <param name="location">The relative x- and y-coordinates on the slider.</param>
        public int SliderValue(TrackBar slider, Point location)
		{
            return MediaPlayer.SliderValue.FromPoint(slider, location.X, location.Y);
        }

        /// <summary>
        /// Returns the slider value at the specified location on the specified slider (trackbar).
        /// </summary>
        /// <param name="slider">The slider whose value should be obtained.</param>
        /// <param name="x">The relative x-coordinate on the slider (for horizontal oriented sliders).</param>
        /// <param name="y">The relative y-coordinate on the slider (for vertical oriented sliders).</param>
        public int SliderValue(TrackBar slider, int x, int y)
        {
            return MediaPlayer.SliderValue.FromPoint(slider, x, y);
        }

#pragma warning restore CA1822 // Mark members as static
    }

    #endregion

    #region Copy Class

    /// <summary>
    /// A class that is used to group together the Copy methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Copy : HideObjectMembers
    {
        #region Fields (Copy Class)

        private const int   NO_ERROR    = 0;

        private Player      _base;
        private bool        _cloneCopy  = true;

        #endregion


        internal Copy(Player player)
        {
            _base = player;
        }


        /// <summary>
        /// Gets or sets a value that specifies whether to use the display clones copy method (which is fast and does not copy overlapping windows) with CopyMode.Video and CopyMode.Display (default: true). When enabled, display overlays are copied according to the Player.DisplayClones.ShowOverlay setting.
        /// </summary>
        public bool CloneMode
        {
            get { return _cloneCopy; }
            set{ _cloneCopy = value; }
        }

        /// <summary>
        /// Gets or sets a value that specifies which part of the screen to copy with the Player.Copy methods (default: CopyMode.Video).
        /// </summary>
        public CopyMode Mode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._copyMode;
            }
            set
            {
                _base._lastError = NO_ERROR;
                _base._copyMode = value;
            }
        }


        /// <summary>
        /// Returns an image from the Player.Copy.Mode part of the screen. See also: Player.Video.ToImage.
        /// </summary>
        public Image ToImage()
        {
            if (_cloneCopy && (_base._copyMode == CopyMode.Display || _base._copyMode == CopyMode.Video))
            {
                return _base.AV_DisplayCopy(_base._copyMode == CopyMode.Video, true);
            }

            Bitmap memoryImage = null;

            if (_base._hasDisplay && (_base._hasVideo || _base._hasOverlayShown))
            {
                Rectangle r;

                switch (_base._copyMode)
                {
                    case CopyMode.Display:
                        r = _base._display.RectangleToScreen(_base._display.DisplayRectangle);
                        break;
                    case CopyMode.Form:
                        r = _base._display.FindForm().RectangleToScreen(_base._display.FindForm().DisplayRectangle);
                        break;
                    case CopyMode.Parent:
                        r = _base._display.Parent.RectangleToScreen(_base._display.Parent.DisplayRectangle);
                        break;
                    case CopyMode.Screen:
                        r = Screen.GetBounds(_base._display);
                        break;

                    default: // CopyMode.Video
                        if (_base._hasVideo) r = _base._display.RectangleToScreen(_base._videoBoundsClip);
                        else r = _base._display.RectangleToScreen(_base._display.DisplayRectangle);
                        break;
                }

                try
                {
                    memoryImage = new Bitmap(r.Width, r.Height);
                    Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                    memoryGraphics.CopyFromScreen(r.Location.X, r.Location.Y, 0, 0, r.Size);
                    memoryGraphics.Dispose();
                    _base._lastError = NO_ERROR;
                }
                catch (Exception e)
                {
                    if (memoryImage != null) { memoryImage.Dispose(); memoryImage = null; }
                    _base._lastError = (HResult)Marshal.GetHRForException(e);
                }
            }
            else
            {
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
            return memoryImage;
        }

        /// <summary>
        /// Returns an image from the specified part of the screen. See also: Player.Video.ToImage.
        /// </summary>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public Image ToImage(CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            Image image = ToImage();
            _base._copyMode = oldMode;
            return image;
        }

        /// <summary>
        /// Returns an image from the Player.Copy.Mode part of the screen with the specified dimensions. See also: Player.Video.ToImage.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio.</param>
        public Image ToImage(int size)
        {
            Image theImage = null;
            if (size >= 8)
            {
                Image copy = ToImage();
                if (copy != null)
                {
                    try
                    {
                        //if (copy.Width > copy.Height) theImage = new Bitmap(copy, size, (size * copy.Height) / copy.Width);
                        //else theImage = new Bitmap(copy, (size * copy.Width) / copy.Height, size);
                        if (copy.Width > copy.Height) theImage = Player.AV_ResizeImage(copy, size, (size * copy.Height) / copy.Width);
                        else theImage = Player.AV_ResizeImage(copy, (size * copy.Width) / copy.Height, size);
                    }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            return theImage;
        }

        /// <summary>
        /// Returns an image from the specified part of the screen with the specified dimensions. See also: Player.Video.ToImage.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public Image ToImage(int size, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            Image image = ToImage(size);
            _base._copyMode = oldMode;
            return image;
        }

        /// <summary>
        /// Returns an image from the Player.Copy.Mode part of the screen with the specified dimensions. See also: Player.Video.ToImage.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public Image ToImage(int width, int height)
        {
            Image theImage = null;
            if (width >= 8 && height >= 8)
            {
                Image copy = ToImage();
                if (copy != null)
                {
                    //try { theImage = new Bitmap(copy, width, height); }
                    try { theImage = Player.AV_ResizeImage(copy, width, height); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            return theImage;
        }

        /// <summary>
        /// Returns an image from the specified part of the screen with the specified dimensions. See also: Player.Video.ToImage.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public Image ToImage(int width, int height, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            Image image = ToImage(width, height);
            _base._copyMode = oldMode;
            return image;
        }


        /// <summary>
        /// Copies an image from the Player.Copy.Mode part of the screen to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        public int ToClipboard()
        {
            Image copy = ToImage();
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies an image from the specified CopyMode part of the screen to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToClipboard(CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToClipboard();
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies an image from the Player.Copy.Mode part of the screen with the specified dimensions to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio</param>
        public int ToClipboard(int size)
        {
            Image copy = ToImage(size);
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies an image from the specified CopyMode part of the screen with the specified dimensions to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        /// <param name="size">The size of the longest side of the image while maintaining the aspect ratio.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToClipboard(int size, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToClipboard(size);
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies an image from the Player.Copy.Mode part of the screen with the specified dimensions to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public int ToClipboard(int width, int height)
        {
            Image copy = ToImage(width, height);
            if (copy != null)
            {
                try { Clipboard.SetImage(copy); }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                copy.Dispose();
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Copies an image from the specified CopyMode part of the screen with the specified dimensions to the system's clipboard. See also: Player.Video.ToClipboard.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToClipboard(int width, int height, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToClipboard(width, height);
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }


        /// <summary>
        /// Saves an image from the Player.Copy.Mode part of the screen to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = ToImage();
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else  _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves an image from the specified CopyMode part of the screen to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToFile(fileName, imageFormat);
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves an image from the Player.Copy.Mode part of the screen with the specified dimensions to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="size">The size of the longest side of the image to save while maintaining the aspect ratio.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int size)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = ToImage(size);
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves an image from the specified CopyMode part of the screen with the specified dimensions to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="size">The size of the longest side of the image to save while maintaining the aspect ratio.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int size, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToFile(fileName, imageFormat, size);
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves an image from the Player.Copy.Mode part of the screen withe the specified dimensions to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="width">The width of the image to save.</param>
        /// <param name="height">The height of the image to save.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int width, int height)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image copy = ToImage(width, height);
                if (copy != null)
                {
                    try { copy.Save(fileName, imageFormat); }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                    copy.Dispose();
                }
            }
            else _base._lastError = HResult.ERROR_INVALID_NAME;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves an image from the specified CopyMode part of the screen with the specified dimensions to the specified file. See also: Player.Video.ToFile.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        /// <param name="width">The width of the image to save.</param>
        /// <param name="height">The height of the image to save.</param>
        /// <param name="mode">A value that indicates the part of the screen to copy. The Player.Copy.Mode setting is not changed.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat, int width, int height, CopyMode mode)
        {
            CopyMode oldMode = _base._copyMode;
            _base._copyMode = mode;
            ToFile(fileName, imageFormat, width, height);
            _base._copyMode = oldMode;
            return (int)_base._lastError;
        }

    }

    #endregion

    #region Sliders Classes

    /// <summary>
    /// A class that is used to group together the Sliders methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Sliders : HideObjectMembers
    {
        #region Fields (Sliders Class)

        private const int       NO_ERROR = 0;

        //private const int       MAX_SCROLL_VALUE = 60000;
        private Player          _base;
        private PositionSlider  _positionSliderClass;

        #endregion

        internal Sliders(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets the shuttle slider (trackbar) that is controlled by the player. Hold down the shift key to decrease the frame step rate and/or the control key to skip step-end playback recovery.
        /// </summary>
        public TrackBar Shuttle
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._shuttleSlider;
			}
			set
			{
				if (value != _base._shuttleSlider)
				{
					if (_base._hasShuttleSlider)
					{
						_base._shuttleSlider.MouseDown -= _base.ShuttleSlider_MouseDown;
						//_base._shuttleSlider.MouseUp      -= _base.ShuttleSlider_MouseUp;
						_base._shuttleSlider.MouseWheel -= _base.ShuttleSlider_MouseWheel;
						_base._shuttleSlider.Scroll -= _base.ShuttleSlider_Scroll;

						_base._shuttleSlider = null;
						_base._hasShuttleSlider = false;
					}

					if (value != null)
					{
						_base._shuttleSlider = value;

						_base._shuttleSlider.SmallChange = 1;
						_base._shuttleSlider.LargeChange = 1;

						_base._shuttleSlider.TickFrequency = 1;

						_base._shuttleSlider.Minimum = -5;
						_base._shuttleSlider.Maximum = 5;
						_base._shuttleSlider.Value = 0;

						_base._shuttleSlider.MouseDown += _base.ShuttleSlider_MouseDown;
						//_base._shuttleSlider.MouseUp      += _base.ShuttleSlider_MouseUp;
						_base._shuttleSlider.MouseWheel += _base.ShuttleSlider_MouseWheel;
						_base._shuttleSlider.Scroll += _base.ShuttleSlider_Scroll;

						//_shuttleSlider.Enabled = _playing;
						_base._hasShuttleSlider = true;
					}
				}
				_base._lastError = NO_ERROR;
			}
		}

		/// <summary>
		/// Gets or sets the audio volume slider (trackbar) that is controlled by the player.
		/// </summary>
		public TrackBar AudioVolume
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._volumeSlider;
            }
			set
			{
				if (value != _base._volumeSlider)
				{
                    if (_base._volumeSlider != null)
                    {
                        _base._volumeSlider.MouseWheel -= _base.VolumeSlider_MouseWheel;
                        _base._volumeSlider.Scroll -= _base.VolumeSlider_Scroll;
                        _base._volumeSlider = null;
                    }

					if (value != null)
					{
						_base._volumeSlider = value;

						_base._volumeSlider.Minimum = 0;
						_base._volumeSlider.Maximum = 100;
						_base._volumeSlider.TickFrequency = 10;
						_base._volumeSlider.SmallChange = 1;
						_base._volumeSlider.LargeChange = 10;

						_base._volumeSlider.Value = (int)(_base._audioVolume * 100);

						_base._volumeSlider.Scroll += _base.VolumeSlider_Scroll;
						_base._volumeSlider.MouseWheel += _base.VolumeSlider_MouseWheel;
					}
				}
				_base._lastError = NO_ERROR;
			}
		}

		/// <summary>
		/// Gets or sets the audio balance slider (trackbar) that is controlled by the player.
		/// </summary>
		public TrackBar AudioBalance
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._balanceSlider;
            }
			set
			{
				if (value != _base._balanceSlider)
				{
					if (_base._balanceSlider != null)
					{
						_base._balanceSlider.MouseWheel -= _base.BalanceSlider_MouseWheel;
						_base._balanceSlider.Scroll -= _base.BalanceSlider_Scroll;
						_base._balanceSlider = null;
					}

                    if (value != null)
					{
						_base._balanceSlider = value;

						_base._balanceSlider.Minimum = -100;
						_base._balanceSlider.Maximum = 100;
						_base._balanceSlider.TickFrequency = 20;
						_base._balanceSlider.SmallChange = 1;
						_base._balanceSlider.LargeChange = 10;

						_base._balanceSlider.Value = (int)(_base._audioBalance * 100);

						_base._balanceSlider.Scroll += _base.BalanceSlider_Scroll;
						_base._balanceSlider.MouseWheel += _base.BalanceSlider_MouseWheel;
					}
				}
				_base._lastError = NO_ERROR;
			}
		}

        /// <summary>
        /// Gets or sets the playback speed slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar Speed
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._speedSlider;
            }
            set
            {
                    if (value != _base._speedSlider)
                    {
                        if (_base._speedSlider != null)
                        {
                            _base._speedSlider.MouseWheel       -= _base.SpeedSlider_MouseWheel;
                            _base._speedSlider.Scroll           -= _base.SpeedSlider_Scroll;
                            //_base._speedSlider.MouseUp        -= _base.SpeedSlider_MouseUp;
                            _base._speedSlider.MouseDown        -= _base.SpeedSlider_MouseDown;

                            _base._speedSlider = null;
                        }

                        if (value != null)
                        {
                            _base._speedSlider = value;

                            _base._speedSlider.Minimum          = 0;
                            _base._speedSlider.Maximum          = 12;
                            _base._speedSlider.TickFrequency    = 1;
                            _base._speedSlider.SmallChange      = 1;
                            _base._speedSlider.LargeChange      = 1;

                            _base.SpeedSlider_ValueToSlider(_base._speed);

                            _base._speedSlider.MouseDown        += _base.SpeedSlider_MouseDown;
                            //_base._speedSlider.MouseUp        += _base.SpeedSlider_MouseUp;
                            _base._speedSlider.Scroll           += _base.SpeedSlider_Scroll;
                            _base._speedSlider.MouseWheel       += _base.SpeedSlider_MouseWheel;
                        }
                    }
                    _base._lastError = NO_ERROR;
            }
        }

		/// <summary>
		/// Returns the slider value at the specified location on the specified slider (trackbar).
		/// </summary>
		/// <param name="slider">The slider whose value should be obtained.</param>
		/// <param name="location">The relative x- and y-coordinates on the slider.</param>
#pragma warning disable CA1822 // Mark members as static
		public int PointToValue(TrackBar slider, Point location)
		{
            return SliderValue.FromPoint(slider, location.X, location.Y);
        }

        /// <summary>
        /// Returns the slider value at the specified location on the specified slider (trackbar).
        /// </summary>
        /// <param name="slider">The slider whose value should be obtained.</param>
        /// <param name="x">The relative x-coordinate on the slider (for horizontal oriented sliders).</param>
        /// <param name="y">The relative y-coordinate on the slider (for vertical oriented sliders).</param>
        public int PointToValue(TrackBar slider, int x, int y)
        {
            return SliderValue.FromPoint(slider, x, y);
        }

        /// <summary>
        /// Returns the location of the specified value on the specified slider (trackbar).
        /// </summary>
        /// /// <param name="slider">The slider whose value location should be obtained.</param>
        /// <param name="value">The value of the slider.</param>
        public Point ValueToPoint(TrackBar slider, int value)
#pragma warning restore CA1822 // Mark members as static
        {
            return SliderValue.ToPoint(slider, value);
        }

        /// <summary>
        /// Provides access to the position slider settings of the player (for example, Player.Sliders.Position.TrackBar).
        /// </summary>
        public PositionSlider Position
        {
            get
            {
                if (_positionSliderClass == null) _positionSliderClass = new PositionSlider(_base);
                return _positionSliderClass;
            }
        }

        /// <summary>
        /// Gets or sets the video image brightness slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar Brightness
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._brightnessSlider;
            }
			set
			{
				if (value != _base._brightnessSlider)
				{
					if (_base._brightnessSlider != null)
					{
						_base._brightnessSlider.MouseWheel -= _base.BrightnessSlider_MouseWheel;
						_base._brightnessSlider.Scroll -= _base.BrightnessSlider_Scroll;
						_base._brightnessSlider = null;
					}

					if (value != null)
					{
						_base._brightnessSlider = value;

						_base._brightnessSlider.Minimum = -100;
						_base._brightnessSlider.Maximum = 100;
						_base._brightnessSlider.TickFrequency = 10;
						_base._brightnessSlider.SmallChange = 1;
						_base._brightnessSlider.LargeChange = 10;

						_base._brightnessSlider.Value = (int)(_base._brightness * 100);

						_base._brightnessSlider.Scroll += _base.BrightnessSlider_Scroll;
						_base._brightnessSlider.MouseWheel += _base.BrightnessSlider_MouseWheel;
					}
				}
				_base._lastError = NO_ERROR;
			}
		}

        /// <summary>
        /// Gets or sets the video image contrast slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar Contrast
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._contrastSlider;
            }
            set
            {
                    if (value != _base._contrastSlider)
                    {
                        if (_base._contrastSlider != null)
                        {
                            _base._contrastSlider.MouseWheel    -= _base.ContrastSlider_MouseWheel;
                            _base._contrastSlider.Scroll        -= _base.ContrastSlider_Scroll;
                            _base._contrastSlider               = null;
                        }

                        if (value != null)
                        {
                            _base._contrastSlider               = value;

                            _base._contrastSlider.Minimum       = -100;
                            _base._contrastSlider.Maximum       = 100;
                            _base._contrastSlider.TickFrequency = 10;
                            _base._contrastSlider.SmallChange   = 1;
                            _base._contrastSlider.LargeChange   = 10;

                            _base._contrastSlider.Value         = (int)(_base._contrast * 100);

                            _base._contrastSlider.Scroll        += _base.ContrastSlider_Scroll;
                            _base._contrastSlider.MouseWheel    += _base.ContrastSlider_MouseWheel;
                        }
                    }
                    _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets the video image hue slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar Hue
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hueSlider;
            }
            set
			{
				if (value != _base._hueSlider)
				{
					if (_base._hueSlider != null)
					{
						_base._hueSlider.MouseWheel -= _base.HueSlider_MouseWheel;
						_base._hueSlider.Scroll -= _base.HueSlider_Scroll;
						_base._hueSlider = null;
					}

					if (value != null)
					{
						_base._hueSlider = value;

						_base._hueSlider.Minimum = -100;
						_base._hueSlider.Maximum = 100;
						_base._hueSlider.TickFrequency = 10;
						_base._hueSlider.SmallChange = 1;
						_base._hueSlider.LargeChange = 10;

						_base._hueSlider.Value = (int)(_base._hue * 100);

						_base._hueSlider.Scroll += _base.HueSlider_Scroll;
						_base._hueSlider.MouseWheel += _base.HueSlider_MouseWheel;
					}
				}
				_base._lastError = NO_ERROR;
			}
		}

        /// <summary>
        /// Gets or sets the video image saturation slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar Saturation
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._saturationSlider;
            }
            set
            {
				if (value != _base._saturationSlider)
				{
					if (_base._saturationSlider != null)
					{
						_base._saturationSlider.MouseWheel -= _base.SaturationSlider_MouseWheel;
						_base._saturationSlider.Scroll -= _base.SaturationSlider_Scroll;
						_base._saturationSlider = null;
					}

					if (value != null)
					{
						_base._saturationSlider = value;

						_base._saturationSlider.Minimum = -100;
						_base._saturationSlider.Maximum = 100;
						_base._saturationSlider.TickFrequency = 10;
						_base._saturationSlider.SmallChange = 1;
						_base._saturationSlider.LargeChange = 10;

						_base._saturationSlider.Value = (int)(_base._saturation * 100);

						_base._saturationSlider.Scroll += _base.SaturationSlider_Scroll;
						_base._saturationSlider.MouseWheel += _base.SaturationSlider_MouseWheel;
					}
				}
				_base._lastError = NO_ERROR;
            }
        }
    }

    /// <summary>
    /// A class that is used to group together the Position Slider methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class PositionSlider : HideObjectMembers
    {
        #region Fields (PositionSlider Class)

        private const int   NO_ERROR          = 0;
        private const int   MAX_SCROLL_VALUE  = 60000;

        private Player      _base;

        #endregion

        internal PositionSlider(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets the media playback position slider (trackbar) that is controlled by the player.
        /// </summary>
        public TrackBar TrackBar
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._positionSlider;
            }
			set
			{
				if (value != _base._positionSlider)
				{
					if (_base._hasPositionSlider)
					{
						if (_base._psTimer != null) _base._timer.Stop();

						_base._positionSlider.MouseDown -= _base.PositionSlider_MouseDown;
						_base._positionSlider.Scroll -= _base.PositionSlider_Scroll;
						_base._positionSlider.MouseWheel -= _base.PositionSlider_MouseWheel;
						_base._positionSlider = null;

						_base._hasPositionSlider = false;
						_base._psTracking = false;
						_base._psValue = 0;
						_base._psBusy = false;
						_base._psSkipped = false;

						if (_base._psTimer != null)
						{
							_base._psTimer.Dispose();
							_base._psTimer = null;
						}
					}

					if (value != null)
					{
						_base._positionSlider = value;
						_base._hasPositionSlider = true;

						_base._psHorizontal = (_base._positionSlider.Orientation == Orientation.Horizontal);
						_base._positionSlider.SmallChange = 0;
						_base._positionSlider.LargeChange = 0;
						_base._positionSlider.TickFrequency = 0;

						SetPositionSliderMode(_base._psHandlesProgress);

						// add events
						_base._positionSlider.MouseDown += _base.PositionSlider_MouseDown;
						_base._positionSlider.Scroll += _base.PositionSlider_Scroll;
						_base._positionSlider.MouseWheel += _base.PositionSlider_MouseWheel;

						if (!_base._playing) _base._positionSlider.Enabled = false;

						_base._psTimer = new Timer { Interval = 100 };
						_base._psTimer.Tick += _base.PositionSlider_TimerTick;
					}
					_base.StartMainTimerCheck();
					_base._lastError = NO_ERROR;
				}
			}
		}

		/// <summary>
		/// Gets or sets the mode (track or progress) of the player's position slider (default: PositionSliderMode.Progress).
		/// </summary>
		public PositionSliderMode Mode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._psHandlesProgress ? PositionSliderMode.Progress : PositionSliderMode.Track;
            }
            set
            {
                _base._lastError = NO_ERROR;
                SetPositionSliderMode(value != PositionSliderMode.Track);
            }
        }

        private void SetPositionSliderMode(bool progressMode)
        {
			_base._psHandlesProgress = progressMode;
			if (_base._hasPositionSlider && _base._playing)
			{
				if (_base._psHandlesProgress)
				{
					_base._positionSlider.Minimum = (int)(_base._startTime * Player.TICKS_TO_MS);
					_base._positionSlider.Maximum = _base._stopTime == 0 ? (_base._mediaLength == 0 ? 10 : (int)(_base._mediaLength * Player.TICKS_TO_MS)) : (int)(_base._stopTime * Player.TICKS_TO_MS);

					//if (_base._playing)
					{
						int pos = (int)(_base.PositionX * Player.TICKS_TO_MS);
						if (pos < _base._positionSlider.Minimum) _base._positionSlider.Value = _base._positionSlider.Minimum;
						else if (pos > _base._positionSlider.Maximum) _base._positionSlider.Value = _base._positionSlider.Maximum;
						else _base._positionSlider.Value = pos;
					}
				}
				else
				{
					_base._positionSlider.Minimum = 0;
					_base._positionSlider.Maximum = _base._mediaLength == 0 ? 10 : (int)(_base._mediaLength * Player.TICKS_TO_MS);
					if (_base._playing) _base._positionSlider.Value = (int)(_base.PositionX * Player.TICKS_TO_MS);
				}
			}
		}

        /// <summary>
        /// Gets or sets a value that indicates whether the player's display window is updated immediately when seeking with the player's position slider (default: false).
        /// </summary>
        public bool LiveUpdate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._psLiveSeek;
            }
            set
            {
                _base._psLiveSeek = value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds that the slider value changes when the scroll box is moved with the mouse wheel (default: 0 (not enabled)).
        /// </summary>
        public int MouseWheel
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._psMouseWheel;
            }
            set
            {
                if (value <= 0) _base._psMouseWheel = 0;
                else if (value > MAX_SCROLL_VALUE) _base._psMouseWheel = MAX_SCROLL_VALUE;
                else _base._psMouseWheel = value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates when the audio output is muted when seeking with the player's position slider (default: SilentSeek.OnMoving).
        /// </summary>
        public SilentSeek SilentSeek
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._psSilentSeek;
            }
            set
            {
                _base._psSilentSeek = value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the thumb of the slider can be immediately dragged when the mouse is clicked anywhere on the slider (default: true).  
        /// </summary>
        public bool ClickAndDrag
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._psClickAndDrag;
            }
            set
            {
                _base._psClickAndDrag = value;
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Removes control by the player of the media playback position slider.
        /// </summary>
        public int Remove()
        {
            TrackBar = null;
            return (int)_base._lastError;
        }
    }

    #endregion

    #region TaskbarProgress Class

    /// <summary>
    /// A class that is used to group together the Taskbar Progress methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class TaskbarProgress : HideObjectMembers
    {
        #region Fields (TaskbarProgress Class)

        private const int               NO_ERROR        = 0;

        private Player                  _base;
        private List<Form>              _taskbarItems;
        internal TaskbarProgressMode    _progressMode;
        private TaskbarProgressState    _taskbarState   = TaskbarProgressState.NoProgress;

        #endregion

        internal TaskbarProgress(Player player)
        {
            _base = player;

            if (!Player._taskbarProgressEnabled)
            {
                Player.TaskbarInstance = (TaskbarIndicator.ITaskbarList3)new TaskbarIndicator.TaskbarInstance();
                Player._taskbarProgressEnabled = true;
            }
            _taskbarItems    = new List<Form>(4);
            _progressMode    = TaskbarProgressMode.Progress;

            _base._lastError = NO_ERROR;
        }

        #region Public - Taskbar Progress methods and properties

        /// <summary>
        /// Adds a taskbar progress indicator to the player.
        /// </summary>
        /// <param name="form">The form whose taskbar item should be added as a progress indicator.</param>
        public int Add(Form form)
        {
            if (Player._taskbarProgressEnabled)
            {
                lock (_taskbarItems)
                {
                    if (form != null)
                    {
                        // check if already exists
                        bool exists = false;
                        for (int i = 0; i < _taskbarItems.Count; i++)
                        {
                            if (_taskbarItems[i] == form)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            _taskbarItems.Add(form);
                            if (_base._playing)
                            {
                                if (_base._paused)
                                {
                                    Player.TaskbarInstance.SetProgressState(form.Handle, TaskbarProgressState.Paused);
                                    SetValue(_base.PositionX);
                                }
                                else if (!_base._fileMode || _base._liveStreamMode)
                                {
                                    Player.TaskbarInstance.SetProgressState(form.Handle, TaskbarProgressState.Indeterminate);
                                }
                            }
                            _base._hasTaskbarProgress = true;
                            _base.StartMainTimerCheck();
                        }
                        _base._lastError = NO_ERROR;
                    }
                    else _base._lastError = HResult.E_INVALIDARG;
                }
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes a taskbar progress indicator from the player.
        /// </summary>
        /// <param name="form">The form whose taskbar progress indicator should be removed.</param>
        public int Remove(Form form)
        {
            if (Player._taskbarProgressEnabled)
            {
                if (_base._hasTaskbarProgress && form != null)
                {
                    lock (_taskbarItems)
                    {
                        for (int index = _taskbarItems.Count - 1; index >= 0; index--)
                        {
                            if (_taskbarItems[index] == form || _taskbarItems[index] == null)
                            {
                                if (_taskbarItems[index] != null)
                                {
                                    Player.TaskbarInstance.SetProgressState(_taskbarItems[index].Handle, TaskbarProgressState.NoProgress);
                                }
                                _taskbarItems.RemoveAt(index);
                            }
                        }

                        if (_taskbarItems.Count == 0)
                        {
                            _taskbarItems = new List<Form>(4);
                        }
                    }
                }
                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes all taskbar progress indicators from the player. Same as Player.TaskbarProgress.Clear.
        /// </summary>
        public int RemoveAll()
        {
            if (Player._taskbarProgressEnabled)
            {
                if (_base._hasTaskbarProgress)
                {
                    _base._hasTaskbarProgress = false;
                    _base.StopMainTimerCheck();
                    SetState(TaskbarProgressState.NoProgress);
                    _taskbarItems    = new List<Form>(4);
                }
                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Removes all taskbar progress indicators from the player. Same as Player.TaskbarProgress.RemoveAll.
        /// </summary>
        public int Clear()
        {
            return RemoveAll();
        }

        /// <summary>
        /// Gets the number of taskbar progress indicators of the player.
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;

                if (_taskbarItems == null)      count = _taskbarItems.Count;

                _base._lastError = NO_ERROR;
                return count;
            }
        }

        /// <summary>
        /// Gets the forms that have a taskbar progress indicator of the player.
        /// </summary>
        public Form[] GetForms
        {
            get
            {
                Form[] result = null;
                if (_taskbarItems != null)
                {
                    int count = _taskbarItems.Count;
                    result = new Form[count];
                    for (int i = 0; i < count; i++)
                    {
                        result[i] = _taskbarItems[i];
                    }
                    _base._lastError = NO_ERROR;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the mode (track or progress) of the player's taskbar progress indicator (default: TaskbarProgressMode.Progress).
        /// </summary>
        public TaskbarProgressMode Mode
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _progressMode;
            }
            set
            {
                _progressMode = value;
                if (_base._hasTaskbarProgress && _base.Playing && _base._fileMode) SetValue(_base.PositionX);
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates how the player's progress indicator is displayed in the taskbar button. Changes when the player's playback status changes.
        /// </summary>
        public TaskbarProgressState State
        {
            get
            {
                if (_base._hasTaskbarProgress)
                {
                    _base._lastError = NO_ERROR;
                    return _taskbarState;
                }
                else
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return TaskbarProgressState.NoProgress;
                }
            }
            set
            {
                if (_base._hasTaskbarProgress)
                {
                    SetState(value);
                    if (!_base._fileMode || _base._liveStreamMode) SetValue(1);
                    _base._lastError = NO_ERROR;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
        }

        /// <summary>
        /// Updates all taskbar progress indicators of the player. Only for use in special cases.
        /// </summary>
        public int Update()
        {
            if (_base._hasTaskbarProgress && _base.Playing)
            {
                SetValue(_base.PositionX);
                if (_base._paused) SetState(TaskbarProgressState.Paused);
            }
            _base._lastError = NO_ERROR;
            return (int)_base._lastError;
        }

        #endregion

        #region Private - SetValue / SetState

        internal void SetValue(long progressValue)
        {
            long pos = progressValue;
            long total;

            if (_taskbarState == TaskbarProgressState.Indeterminate) return;

            if (!_base._fileMode || _base._liveStreamMode)
            {
                pos     = 1;
                total   = 1;
            }
            else
            {
                if (_progressMode == TaskbarProgressMode.Track)
                {
                    total = _base._mediaLength;
                }
                else
                {
                    if (pos < _base._startTime)
                    {
                        total = _base._stopTime == 0 ? _base._mediaLength : _base._stopTime;
                    }
                    else
                    {
                        if (_base._stopTime == 0) total = _base._mediaLength - _base._startTime;
                        else
                        {
                            if (pos <= _base._stopTime) total = _base._stopTime - _base._startTime;
                            else total = _base._mediaLength - _base._startTime;
                        }
                        pos -= _base._startTime;
                    }
                }
            }

            for (int i = 0; i < _taskbarItems.Count; i++)
            {
                if (_taskbarItems[i] != null)
                {
                    try { Player.TaskbarInstance.SetProgressValue(_taskbarItems[i].Handle, (ulong)pos, (ulong)total); }
                    catch { _taskbarItems[i] = null; }
                }
            }
        }

        internal void SetState(TaskbarProgressState taskbarState)
        {
            //if (_taskbarItems.Count > 0)
            {
                _taskbarState = taskbarState;
                for (int i = 0; i < _taskbarItems.Count; i++)
                {
                    if (_taskbarItems[i] != null)
                    {
                        try { Player.TaskbarInstance.SetProgressState(_taskbarItems[i].Handle, taskbarState); }
                        catch { _taskbarItems[i] = null; }
                    }
                }
            }
        }

        #endregion
    }

    #endregion

    #region SystemPanels Class

    /// <summary>
    /// A class that is used to group together the System Panels methods of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class SystemPanels : HideObjectMembers
    {
        #region Fields (SystemPanels Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal SystemPanels(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Opens the System Audio Mixer Control Panel.
        /// </summary>
        public bool ShowAudioMixer()
        {
            return ShowAudioMixer(null);
        }

        /// <summary>
        /// Opens the System Audio Mixer Control Panel.
        /// </summary>
        /// <param name="centerForm">The control panel is centered on top of the specified form.</param>
        public bool ShowAudioMixer(Form centerForm)
        {
            _base._lastError = NO_ERROR;
            return SafeNativeMethods.CenterSystemDialog("SndVol.exe", "", centerForm) || SafeNativeMethods.CenterSystemDialog("SndVol32.exe", "", centerForm);
        }

        /// <summary>
        /// Opens the System Sound Control Panel.
        /// </summary>
        public bool ShowAudioDevices()
        {
            return ShowAudioDevices(null);
        }

        /// <summary>
        /// Opens the System Sound Control Panel.
        /// </summary>
        /// <param name="centerForm">The control panel is centered on top of the specified form.</param>
        public bool ShowAudioDevices(Form centerForm)
        {
            _base._lastError = NO_ERROR;
            return SafeNativeMethods.CenterSystemDialog("control", "mmsys.cpl,,0", centerForm);
        }

        /// <summary>
        /// Opens the System Sound Control Panel.
        /// </summary>
        public bool ShowAudioInputDevices()
        {
            return ShowAudioInputDevices(null);
        }

        /// <summary>
        /// Opens the System Sound Control Panel.
        /// </summary>
        /// <param name="centerForm">The control panel is centered on top of the specified form.</param>
        public bool ShowAudioInputDevices(Form centerForm)
        {
            _base._lastError = NO_ERROR;
            return SafeNativeMethods.CenterSystemDialog("control", "mmsys.cpl,,1", centerForm);
        }

        /// <summary>
        /// Opens the System Display Control Panel.
        /// </summary>
        public bool ShowDisplaySettings()
        {
            return ShowDisplaySettings(null);
        }

        /// <summary>
        /// Opens the System Display Control Panel.
        /// </summary>
        /// <param name="centerForm">The control panel is centered on top of the specified form.</param>
        public bool ShowDisplaySettings(Form centerForm)
        {
            _base._lastError = NO_ERROR;
            return SafeNativeMethods.CenterSystemDialog("control", "desk.cpl", centerForm);
        }
    }

    #endregion

    #region Subtitles Class

    /// <summary>
    /// A class that is used to group together the Subtitles methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Subtitles : HideObjectMembers
    {
        #region Fields (Subtitles Class)

        private const int   NO_ERROR            = 0;
        private const int   MAX_DIRECTORY_DEPTH = 3;

        private Player      _base;

        #endregion

        internal Subtitles(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets a value that indicates whether the player's subtitles are activated (by subscribing to the Player.Events.MediaSubtitleChanged event) (default: false).
        /// </summary>
        public bool Enabled
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_SubtitlesEnabled;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media (or the media specified with Player.Subtitles.Filename) has a subtitles (.srt) file.
        /// </summary>
        public bool Exists
        {
            get
            {
                _base._lastError = NO_ERROR;
                //return _base.Subtitles_Exists() != string.Empty;
                return _base.Subtitles_Exists().Length > 0;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has active subtitles.
        /// </summary>
        public bool Present
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_HasSubtitles;
            }
        }

        /// <summary>
        /// Gets the number of subtitles in the player's active subtitles.
        /// </summary>
        public int Count
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_HasSubtitles ? _base.st_SubTitleCount : 0;
            }
        }

        /// <summary>
        /// Gets the index of the current subtitle in the player's active subtitles.
        /// </summary>
        public int Current
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_HasSubtitles ? _base.st_CurrentIndex : 0;
            }
        }

        /// <summary>
        /// Returns the text of the current subtitle (usually obtained with the Player.Events.MediaSubtitleChanged event).
        /// </summary>
        public string GetText()
        {
            _base._lastError = NO_ERROR;
            return _base.st_SubtitleOn ? _base.st_SubtitleItems[_base.st_CurrentIndex].Text : string.Empty;
        }

        /// <summary>
        /// Returns the start time (including Player.Subtitles.TimeShift) of the current subtitle.
        /// </summary>
        public TimeSpan GetStartTime()
        {
            _base._lastError = NO_ERROR;
            return _base.st_SubtitleOn ? TimeSpan.FromTicks(_base.st_SubtitleItems[_base.st_CurrentIndex].StartTime + _base.st_TimeShift) : TimeSpan.Zero;
        }

        /// <summary>
        /// Returns the end time (including Player.Subtitles.TimeShift) of the current subtitle.
        /// </summary>
        public TimeSpan GetEndTime()
        {
            _base._lastError = NO_ERROR;
            return _base.st_SubtitleOn ? TimeSpan.FromTicks(_base.st_SubtitleItems[_base.st_CurrentIndex].EndTime + _base.st_TimeShift) : TimeSpan.Zero;
        }

        /// <summary>
        /// Returns the text of the specified item in the player's active subtitles.
        /// </summary>
        /// <param name="index">The index of the item in the player's active subtitles.</param>
        public string GetText(int index)
        {
            _base._lastError = NO_ERROR;
            if (_base.st_HasSubtitles && index >= 0 && index < _base.st_SubTitleCount) return _base.st_SubtitleItems[index].Text;
            return string.Empty;
        }

        /// <summary>
        /// Returns the start time (including Player.Subtitles.TimeShift) of the specified item in the player's active subtitles.
        /// </summary>
        /// <param name="index">The index of the item in the player's active subtitles.</param>
        public TimeSpan GetStartTime(int index)
        {
            _base._lastError = NO_ERROR;
            if (_base.st_HasSubtitles && index >= 0 && index < _base.st_SubTitleCount) return TimeSpan.FromTicks(_base.st_SubtitleItems[index].StartTime + _base.st_TimeShift);
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Returns the end time (including Player.Subtitles.TimeShift) of the specified item in the player's active subtitles.
        /// </summary>
        /// <param name="index">The index of the item in the player's active subtitles.</param>
        public TimeSpan GetEndTime(int index)
        {
            if (_base.st_HasSubtitles && index >= 0 && index < _base.st_SubTitleCount) return TimeSpan.FromTicks(_base.st_SubtitleItems[index].EndTime + _base.st_TimeShift);
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Returns the path and file name of the player's active subtitles file.
        /// </summary>
        public string GetFileName()
        {
            _base._lastError = NO_ERROR;
            if (_base.st_HasSubtitles) return _base.st_SubtitlesName;
            return string.Empty;
        }

        /// <summary>
        /// Gets or sets the text encoding of subtitles (default: Encoding.Default).
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_Encoding;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base.st_Encoding)
                {
                    _base.st_Encoding = value;
                    if (_base.st_SubtitlesEnabled && _base._playing) _base.Subtitles_Start(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the initial directory to search for subtitles files (default: string.Empty (the directory of the playing media)).
        /// </summary>
        public string Directory
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_base.st_Directory == null) _base.st_Directory = string.Empty;
                return _base.st_Directory;
            }
            set
            {
                _base._lastError = HResult.E_INVALIDARG;
                if (!string.IsNullOrWhiteSpace(value) && System.IO.Directory.Exists(value))
                {
                    try
                    {
                        _base.st_Directory = Path.GetDirectoryName(value);
                        if (_base.st_SubtitlesEnabled && _base._playing) _base.Subtitles_Start(true);
                        _base._lastError = NO_ERROR;
                    }
                    catch (Exception e)
                    {
                        _base.st_Directory = string.Empty;
                        _base._lastError = (HResult)Marshal.GetHRForException(e);
                    }
                }
                else _base.st_Directory = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the number of nested directories to search for subtitles files (values 0 to 3, default: 0 (base directory only)).
        /// </summary>
        public int DirectoryDepth
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_DirectoryDepth;
            }
            set
            {
                _base._lastError = NO_ERROR;

                if (value <= 0) value = 0;
                else if (value >= MAX_DIRECTORY_DEPTH) value = MAX_DIRECTORY_DEPTH;
                if (value != _base.st_DirectoryDepth)
                {
                    _base.st_DirectoryDepth = value;
                    if (_base.st_SubtitlesEnabled && _base._playing && !_base.st_HasSubtitles) _base.Subtitles_Start(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the file name of the subtitles file to search for (without directory and extension) (default: string.Empty (the file name of the playing media)). Reset to string.Empty when media starts playing.
        /// </summary>
        public string FileName
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_FileName;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        _base.st_FileName = Path.GetFileNameWithoutExtension(value) + Player.SUBTITLES_FILE_EXTENSION;
                        if (_base.st_SubtitlesEnabled && _base._playing) _base.Subtitles_Start(true);
                    }
                    catch (Exception e)
                    {
                        _base.st_FileName = string.Empty;
                        _base._lastError = (HResult)Marshal.GetHRForException(e);
                    }
                }
                else _base.st_FileName = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the number of milliseconds that subtitles appear earlier (negative values) or later (positive values) than specified by the subtitles data. Reset when media ends playing.
        /// </summary>
        public int TimeShift
        {
            get
            {
                _base._lastError = NO_ERROR;
                return (int)(_base.st_TimeShift * Player.TICKS_TO_MS);
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base.st_TimeShift)
                {
                    _base.st_TimeShift = value * Player.MS_TO_TICKS; // no check (?)
                    if (_base.st_HasSubtitles) _base.OnMediaPositionChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether any HTML tags are removed from subtitles (default: true).
        /// </summary>
        public bool RemoveHTMLTags
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_RemoveHTMLTags;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base.st_RemoveHTMLTags)
                {
                    _base.st_RemoveHTMLTags = value;
                    if (_base.st_HasSubtitles) _base.Subtitles_Start(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether audio only media can activate subtitles (default: false).
        /// </summary>
        public bool AudioOnly
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_AudioOnlyEnabled;
            }
            set
            {
                if (value != _base.st_AudioOnlyEnabled)
                {
                    _base._lastError = NO_ERROR;
                    _base.st_AudioOnlyEnabled = value;
                    if (_base.st_SubtitlesEnabled && _base._playing && !_base._hasVideo)
                    {
                        if (_base.st_AudioOnlyEnabled) _base.Subtitles_Start(true);
                        else
                        {
                            if (_base.st_HasSubtitles) _base.Subtitles_Stop();
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Position Class

    /// <summary>
    /// A class that is used to group together the Position methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Position : HideObjectMembers
    {
        #region Fields (Position Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Position(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media, measured from the (natural) beginning of the media.
        /// </summary>
        public TimeSpan FromBegin
        {
            get
            {
                if (_base._playing)
                {
                    _base._lastError = NO_ERROR;
                    if (!_base._fileMode) return TimeSpan.FromTicks(_base.PositionX - _base._deviceStart);
                    else return TimeSpan.FromTicks(_base.PositionX);
                }
                else
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return TimeSpan.Zero;
                }
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else _base.SetPosition(value.Ticks);
            }
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media, measured from the (natural) end of the media.
        /// </summary>
        public TimeSpan ToEnd
        {
            get
            {
                long toEnd = 0;

                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else
                {
                    toEnd = _base._mediaLength - _base.PositionX;
                    if (toEnd < 0) toEnd = 0;
                    _base._lastError = NO_ERROR;
                }
                return TimeSpan.FromTicks(toEnd);
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else _base.SetPosition(_base._mediaLength - value.Ticks);
            }
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media, measured from its (adjustable) start time. See also: Player.Media.StartTime.
        /// </summary>
        public TimeSpan FromStart
        {
            get
            {
                if (_base._playing)
                {
                    _base._lastError = NO_ERROR;
                    if (!_base._fileMode) return TimeSpan.FromTicks(_base.PositionX - _base._deviceStart);
                    else return TimeSpan.FromTicks(_base.PositionX - _base._startTime);
                }
                else
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return TimeSpan.Zero;
                }
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else _base.SetPosition(_base._startTime + value.Ticks);
            }
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media, measured from its (adjustable) stop time. See also: Player.Media.StopTime.
        /// </summary>
        public TimeSpan ToStop
        {
            get
            {
                long toEnd = 0;

                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else
                {
                    if (_base._stopTime == 0)
                    {
                        toEnd = _base._mediaLength - _base.PositionX;
                        if (toEnd < 0) toEnd = 0;
                    }
                    else toEnd = _base._stopTime - _base.PositionX;
                    _base._lastError = NO_ERROR;
                }
                return TimeSpan.FromTicks(toEnd);
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else _base.SetPosition(_base._stopTime - value.Ticks);
            }
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media relative to its (natural) begin and end time. Values from 0.0 to 1.0. See also: Player.Position.Progress.
        /// </summary>
        public float Track
        {
            get
            {
                if (!_base._fileMode || !_base._playing || _base._mediaLength <= 0)
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return 0;
                }
                else
                {
                    _base._lastError = NO_ERROR;
                    return (float)_base.PositionX / _base._mediaLength;
                }
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else
                {
                    if (value >= 0 && value < 1)
                    {
                        _base.SetPosition((long)(value * _base._mediaLength));
                    }
                    else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
            }
        }

        /// <summary>
        /// Gets or sets the playback position of the playing media relative to its (adjustable) start and stop time. Values from 0.0 to 1.0. See also: Player.Position.Track.
        /// </summary>
        public float Progress
        {
            get
            {
                if (!_base._fileMode || !_base._playing)
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return 0;
                }
                else
                {
                    _base._lastError = NO_ERROR;

                    long pos = _base._stopTime == 0 ? _base._mediaLength : _base._stopTime;
                    if (pos == 0 || pos <= _base._startTime) return 0;

                    float pos2 = (_base.PositionX - _base._startTime) / (pos - _base._startTime);
                    if (pos2 < 0) return 0;
                    return pos2 > 1 ? 1 : pos2;
                }
            }
            set
            {
                if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                else
                {
                    if (value >= 0 && value < 1)
                    {
                        _base._lastError = NO_ERROR;

                        long pos = _base._stopTime == 0 ? _base._mediaLength : _base._stopTime;
                        if (pos <= _base._startTime) return;

                        _base.SetPosition((long)(value * (pos - _base._startTime)) + _base._startTime);
                    }
                    else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
            }
        }

        /// <summary>
        /// Rewinds the playback position of the playing media to its (adjustable) start time. See also: Player.Media.StartTime.
        /// </summary>
        public int Rewind()
        {
            if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            else _base.SetPosition(_base._startTime);
            return (int)_base._lastError;
        }

        /// <summary>
        /// Changes the playback position of the playing media in any direction by the given amount of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to skip. Use a negative value to skip backwards.</param>
        public int Skip(int seconds)
        {
            if (!_base._fileMode || !_base._playing) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            else _base.SetPosition(_base.PositionX + (seconds * Player.ONE_SECOND_TICKS));
            return (int)_base._lastError;
        }

        /// <summary>
        /// Changes the playback position of the playing media in any direction by the given amount of video frames. The result can differ from the specified value. See also: Player.Position.StepEnd.
        /// </summary>
        /// <param name="frames">The number of frames to step. Use a negative value to step backwards.</param>
        public int Step(int frames)
        {
            if (!_base._fileMode || !_base._playing)
            {
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                return (int)_base._lastError;
            }
            else return _base.Step(frames);
        }

        /// <summary>
        /// Gets or sets a value that indicates the margin at the end of media files that is used to avoid stepping past the end of media files using the Player.Position.Step method. Values from 10 to 1000 milliseconds (default: 200).
        /// </summary>
        public int StepEOFMargin
        {
            get
            {
                _base._lastError = NO_ERROR;
                return (int)(_base._stepMargin * Player.TICKS_TO_MS);
            }
            set
            {
                if (value < 10 || value > 1000)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    _base._stepMargin = value * Player.MS_TO_TICKS;
                    _base._lastError = NO_ERROR;
                }
            }
        }

        /// <summary>
        /// Restores the player's video image after using the Player.Position.Step method while the player is paused (step-end playback recovery).
        /// </summary>
		public int StepEnd()
		{
			if (_base._stepMode && _base._hasVideo && _base._paused) _base.AV_UpdateTopology();

			_base._lastError = NO_ERROR;
            return NO_ERROR;
        }
	}

    #endregion

    #region Media Class

    /// <summary>
    /// A class that is used to group together the Media methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class Media : HideObjectMembers
	{
        #region Fields (Media Class)

        private const int       NO_ERROR            = 0;

        private Player          _base;

        // Media album art information
        private Image           _tagImage;
        private DirectoryInfo   _directoryInfo;
        private string[]        _searchKeyWords     = { "*front*", "*cover*" }; // , "*albumart*large*" };
        private string[]        _searchExtensions   = { ".jpg", ".png", ".jpeg", ".bmp", ".gif", ".tiff", ".tif", ".jfif" };

        #endregion

        internal Media(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets a value that indicates the source type of the playing media, such as a local file or a webcam.
        /// </summary>
        public MediaSourceType SourceType
        {
            get
            {
                _base._lastError = NO_ERROR;
                MediaSourceType source = MediaSourceType.None;

                if (_base._playing) source = _base.AV_GetSourceType();
                return source;
            }
        }

        /// <summary>
        /// Gets a value that indicates the source category of the playing media, such as local files or local capture devices.
        /// </summary>
        public MediaSourceCategory SourceCategory
        {
            get
            {
                _base._lastError = NO_ERROR;
                MediaSourceCategory source = MediaSourceCategory.None;

                if (_base._playing) source = _base.AV_GetSourceCategory();
                return source;
            }
        }

        /// <summary>
        /// Gets the natural length (duration) of the playing media. See also: Player.Media.Duration and Player.Media.GetDuration.
        /// </summary>
        public TimeSpan Length
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (!_base._playing || !_base._fileMode) return TimeSpan.Zero;
                return TimeSpan.FromTicks(_base._mediaLength);
            }
        }

        /// <summary>
        /// Gets the duration of the playing media from the (adjustable) start time to the stop time. See also: Player.Media.Length and Player.Media.GetDuration.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (!_base._playing || !_base._fileMode) return TimeSpan.Zero;
                return _base._stopTime == 0 ? TimeSpan.FromTicks(_base._mediaLength - _base._startTime) : TimeSpan.FromTicks(_base._stopTime - _base._startTime);
            }
        }

        /// <summary>
        /// Returns the duration of the specified part of the playing media. See also: Player.Media.Length and Player.Media.Duration.
        /// </summary>
        /// <param name="part">Specifies the part of the playing media whose duration is to be obtained.</param>
        public TimeSpan GetDuration(MediaPart part)
        {
            _base._lastError = NO_ERROR;

            if (!_base._playing || !_base._fileMode) return TimeSpan.Zero;

            switch (part)
            {
                case MediaPart.BeginToEnd:
                    return TimeSpan.FromTicks(_base._mediaLength);
                //break;

                case MediaPart.StartToStop:
                    return _base._stopTime == 0 ? TimeSpan.FromTicks(_base._mediaLength - _base._startTime) : TimeSpan.FromTicks(_base._stopTime - _base._startTime);
                //break;

                case MediaPart.FromStart:
                    return TimeSpan.FromTicks(_base.PositionX - _base._startTime);
                //break;

                case MediaPart.ToEnd:
                    return TimeSpan.FromTicks(_base._mediaLength - _base.PositionX);
                //break;

                case MediaPart.ToStop:
                    return _base._stopTime == 0 ? TimeSpan.FromTicks(_base._mediaLength - _base.PositionX) : TimeSpan.FromTicks(_base._stopTime - _base.PositionX);
                //break;

                //case MediaLength.FromBegin:
                default:
                    return (TimeSpan.FromTicks(_base.PositionX));
                    //break;
            }
        }

        /// <summary>
        /// Returns the specified part of the file name or device name of the playing media.
        /// </summary>
        /// <param name="part">Specifies the part of the name to return.</param>
        public string GetName(MediaName part)
        {
            string mediaName = string.Empty;
            _base._lastError = NO_ERROR;

            if (!_base._fileMode && !_base._liveStreamMode)
            {
                if (part == MediaName.FileName || part == MediaName.FileNameWithoutExtension) mediaName = _base._fileName;
            }
            else if (_base._playing)
            {
                try
                {
                    switch (part)
                    {
                        case MediaName.FileName:
                            mediaName = Path.GetFileName(_base._fileName);
                            break;
                        case MediaName.DirectoryName:
                            mediaName = Path.GetDirectoryName(_base._fileName);
                            break;
                        case MediaName.PathRoot:
                            mediaName = Path.GetPathRoot(_base._fileName);
                            break;
                        case MediaName.Extension:
                            mediaName = Path.GetExtension(_base._fileName);
                            break;
                        case MediaName.FileNameWithoutExtension:
                            mediaName = Path.GetFileNameWithoutExtension(_base._fileName);
                            break;

                        default: // case MediaName.FullPath:
                            mediaName = _base._fileName;
                            break;
                    }
                }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
            }
            return mediaName;
        }

        /// <summary>
        /// Gets the number of audio tracks in the playing media. See also: Player.Media.GetAudioTracks.
        /// </summary>
        public int AudioTrackCount
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._audioTrackCount;
            }
        }

        /// <summary>
        /// Returns the audio tracks in the playing media or null if none are present. See also: Player.Media.AudioTrackCount.
        /// </summary>
        public AudioTrack[] GetAudioTracks()
        {
            return _base.AV_GetAudioTracks();
        }

        /// <summary>
        /// Gets the number of video tracks in the playing media. See also: Player.Media.GetVideoTracks.
        /// </summary>
        public int VideoTrackCount
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._videoTrackCount;
            }
        }

        /// <summary>
        /// Returns the video tracks in the playing media or null if none are present. See also: Player.Media.VideoTrackCount.
        /// </summary>
        public VideoTrack[] GetVideoTracks()
        {
            return _base.AV_GetVideoTracks();
        }

        /// <summary>
        /// Gets the original size (width and height) of the video image of the playing media, in pixels.
        /// </summary>
        public Size VideoSourceSize
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo ? _base._videoSourceSize : Size.Empty;
            }
        }

        /// <summary>
        /// Gets the video frame rate of the playing media, in frames per second.
        /// </summary>
        public float VideoFrameRate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo ? _base._videoFrameRate : 0;
            }
        }

        /// <summary>
        /// Gets or sets the (repeat) start time of the playing media. The start time can also be set with the Player.Play method. Changing the start time ends the chapter playback of the playing media. See also: Player.Media.StopTime.
        /// </summary>
        public TimeSpan StartTime
        {
            get
            {
                _base._lastError = NO_ERROR;
                return TimeSpan.FromTicks(_base._startTime);
            }
            set
            {
                if (!_base._playing || !_base._fileMode)
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return;
                }

                _base._lastError = NO_ERROR;

                if (_base._chapterMode)
                {
                    _base.AV_StopChapters(value, TimeSpan.Zero);
                }
                else
                {
                    long newStart = value.Ticks;

                    if (_base._startTime == newStart) return;
                    if ((_base._stopTime != 0 && newStart >= _base._stopTime) || newStart >= _base._mediaLength)
                    {
                        _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                        return;
                    }

                    if (_base._hasPositionSlider && _base._psHandlesProgress)
                    {
                        _base._positionSlider.Minimum = (int)(newStart * Player.TICKS_TO_MS);
                    }

                    _base._startTime = newStart;
                }

                if (_base._startTime > _base.PositionX) _base.PositionX = _base._startTime;
                _base._mediaStartStopTimeChanged?.Invoke(_base, EventArgs.Empty);
            }
        }

		/// <summary>
		/// Gets or sets the (repeat) stop time of the playing media. The stop time can also be set with the Player.Play method.
		/// TimeSpan.Zero or 00:00:00 indicates the natural end of the media. Changing the stop time ends the chapter playback of the playing media. See also: Player.Media.StartTime.
		/// </summary>
		public TimeSpan StopTime
        {
            get
            {
                _base._lastError = NO_ERROR;
                return TimeSpan.FromTicks(_base._stopTime);
            }
            set
            {
                if (!_base._playing || !_base._fileMode)
                {
                    _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                    return;
                }

                _base._lastError = NO_ERROR;

                if (_base._chapterMode)
                {
                    _base.AV_StopChapters(TimeSpan.Zero, value);
                }
                else
                {
                    long newStop = value.Ticks;

                    if (_base._stopTime == newStop) return;
                    if ((newStop != 0 && newStop < _base._startTime) || newStop >= _base._mediaLength)
                    {
                        _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                        return;
                    }

                    if (_base._hasPositionSlider && _base._psHandlesProgress)
                    {
                        _base._positionSlider.Maximum = newStop == 0 ? (int)(_base._mediaLength * Player.TICKS_TO_MS) : (int)(newStop * Player.TICKS_TO_MS);
                    }

                    _base._stopTime = newStop;
                    _base.AV_UpdateTopology();
                }
                _base._mediaStartStopTimeChanged?.Invoke(_base, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Returns metadata (media information such as title and artist name) of the playing media. See also: Player.Media.GetAudioTracks and .GetVideoTracks track information.
        /// </summary>
        public Metadata GetMetadata()
        {
            return GetMetadata(ImageSource.MediaOrFolder);
        }

        /// <summary>
        /// Returns metadata (media information such as title and artist name) of the playing media. See also: Player.Media.GetAudioTracks and .GetVideoTracks track information.
        /// </summary>
        /// <param name="imageSource">A value indicating whether and where to obtain an image related to the media.</param>
        public Metadata GetMetadata(ImageSource imageSource)
        {
            if (_base._playing)
            {
                if (_base._fileMode)
                {
                    return GetMetadata(_base._fileName, imageSource);
                }
                else
                {
                    _base._lastError = NO_ERROR;

                    Metadata data = new Metadata
                    {
                        _album = GetName(MediaName.FullPath),
                        _title = GetName(MediaName.FileNameWithoutExtension)
                    };

                    if (_base._liveStreamMode && !string.IsNullOrWhiteSpace(data._title) && data._title.Length > 1)
                    {
                        data._title = char.ToUpper(data._title[0]) + data._title.Substring(1);
                    }

                    return data;
                }
            }
            else
            {
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                return new Metadata();
            }
        }

        /// <summary>
        /// Returns metadata (media information such as title and artist name) of the specified media file. See also: Player.Media.GetAudioTracks and .GetVideoTracks track information.
        /// </summary>
        /// <param name="fileName">The path and name of the media file from which the metadata is to be obtained.</param>
        /// <param name="imageSource">A value indicating whether and where to obtain an image related to the media file.</param>
        public Metadata GetMetadata(string fileName, ImageSource imageSource)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _base._lastError = HResult.E_INVALIDARG;
                return new Metadata();
            }

            Metadata tagInfo;
            _base._lastError = NO_ERROR;

            try
            {
                if (!new Uri(fileName).IsFile)
                {
                    tagInfo = new Metadata();
                    {
                        try
                        {
                            tagInfo._title = Path.GetFileNameWithoutExtension(fileName);
                            tagInfo._album = fileName;
                        }
                        catch { /* ignored */ }
                    }
                    return tagInfo;
                }
            }
            catch { /* ignored */ }

            tagInfo = GetMediaTags(fileName, imageSource);

            try
            {
                // Get info from file path
                //if (tagInfo._artist == null || tagInfo._artist.Length == 0) tagInfo._artist = Path.GetFileName(Path.GetDirectoryName(fileName));
                if (tagInfo._title == null || tagInfo._title.Length == 0) tagInfo._title = Path.GetFileNameWithoutExtension(fileName);

                // Get album art image (with certain values of 'imageSource')
                if (imageSource == ImageSource.FolderOrMedia || imageSource == ImageSource.FolderOnly || (imageSource == ImageSource.MediaOrFolder && tagInfo._image == null))
                {
                    GetMediaImage(fileName);
                    if (_tagImage != null) // null image not to replace image retrieved from media file with FolderOrMedia
                    {
                        tagInfo._image = _tagImage;
                        _tagImage = null;
                    }
                }
            }
            catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }

            return tagInfo;
        }

		private static Metadata GetMediaTags(string fileName, ImageSource imageSource)
        {
            Metadata        tagInfo     = new Metadata();
            IMFMediaSource  mediaSource = null;
            IPropertyStore  propStore   = null;
            PropVariant     propVariant = new PropVariant();

            HResult result = MFExtern.MFCreateSourceResolver(out IMFSourceResolver sourceResolver);
            if (result == NO_ERROR)
            {
                try
                {
                    result = sourceResolver.CreateObjectFromURL(fileName, MFResolution.MediaSource, null, out MFObjectType objectType, out object source);
                    if (result == NO_ERROR)
                    {
                        mediaSource = (IMFMediaSource)source;

                        result = MFExtern.MFGetService(mediaSource, MFServices.MF_PROPERTY_HANDLER_SERVICE, typeof(IPropertyStore).GUID, out object store);
                        if (result == NO_ERROR)
                        {
                            propStore = (IPropertyStore)store;

                            // Artist
                            result = propStore.GetValue(PropertyKeys.PKEY_Music_Artist, propVariant);
                            tagInfo._artist = propVariant.GetString();

                            // Album Artist
                            propStore.GetValue(PropertyKeys.PKEY_Music_AlbumArtist, propVariant);
                            tagInfo._albumArtist = propVariant.GetString();

                            // Title
                            propStore.GetValue(PropertyKeys.PKEY_Title, propVariant);
                            tagInfo._title = propVariant.GetString();

                            // SubTitle
                            propStore.GetValue(PropertyKeys.PKEY_Media_SubTitle, propVariant);
                            tagInfo._subTitle = propVariant.GetString();

                            // Album
                            propStore.GetValue(PropertyKeys.PKEY_Music_AlbumTitle, propVariant);
                            tagInfo._album = propVariant.GetString();

                            // Genre
                            propStore.GetValue(PropertyKeys.PKEY_Music_Genre, propVariant);
                            tagInfo._genre = propVariant.GetString();

                            // Duration
                            propStore.GetValue(PropertyKeys.PKEY_Media_Duration, propVariant);
                            tagInfo._duration = TimeSpan.FromTicks((long)propVariant.GetULong());

                            // TrackNumber
                            propStore.GetValue(PropertyKeys.PKEY_Music_TrackNumber, propVariant);
                            tagInfo._trackNumber = (int)propVariant.GetUInt();

                            // Year
                            propStore.GetValue(PropertyKeys.PKEY_Media_Year, propVariant);
                            tagInfo._year = propVariant.GetUInt().ToString();

                            // Comment
                            propStore.GetValue(PropertyKeys.PKEY_Comment, propVariant);
                            tagInfo._comment = propVariant.GetString();

                            // Image
                            if (imageSource != ImageSource.None && imageSource != ImageSource.FolderOnly)
                            {
                                propStore.GetValue(PropertyKeys.PKEY_ThumbnailStream, propVariant);
                                if (propVariant.ptr != null)
                                {
                                    IStream stream = (IStream)Marshal.GetObjectForIUnknown(propVariant.ptr);

                                    stream.Stat(out System.Runtime.InteropServices.ComTypes.STATSTG streamInfo, STATFLAG.NoName);

                                    int streamSize = (int)streamInfo.cbSize;
                                    if (streamSize > 0)
                                    {
                                        byte[] buffer = new byte[streamSize];
                                        stream.Read(buffer, streamSize, IntPtr.Zero);

                                        MemoryStream ms = new MemoryStream(buffer, false);
                                        Image image = Image.FromStream(ms);

                                        tagInfo._image = new Bitmap(image);

                                        image.Dispose();
                                        ms.Dispose();
                                    }

                                    Marshal.ReleaseComObject(streamInfo);
                                    Marshal.ReleaseComObject(stream);
                                }
                            }
                        }
                    }
                }
                //catch (Exception e) { result = (HResult)Marshal.GetHRForException(e); }
                catch { /* ignored */ }
            }

            if (sourceResolver != null) Marshal.ReleaseComObject(sourceResolver);
            if (mediaSource != null) Marshal.ReleaseComObject(mediaSource);
            if (propStore != null) Marshal.ReleaseComObject(propStore);
            propVariant.Dispose();

            return tagInfo;
        }

        // Get media information image help function
        private void GetMediaImage(string fileName)
        {
            _directoryInfo              = new DirectoryInfo(Path.GetDirectoryName(fileName));
            string searchFileName       = Path.GetFileNameWithoutExtension(fileName);
            string searchDirectoryName  = _directoryInfo.Name;

            // 1. search using the file name
            if (!SearchMediaImage(searchFileName))
            {
                // 2. search using the directory name
                if (!SearchMediaImage(searchDirectoryName))
                {
                    // 3. search using keywords
                    int i = 0;
                    bool result;
                    do result = SearchMediaImage(_searchKeyWords[i++]);
                    while (!result && i < _searchKeyWords.Length);

                    if (!result)
                    {
                        // 4. find largest file
                        SearchMediaImage("*");
                    }
                }
            }
            _directoryInfo = null;
        }

        // Get media image help function
        private bool SearchMediaImage(string searchName)
        {
            if (searchName.EndsWith(@":\")) return false; // root directory - no folder name (_searchDirectoryName, for example C:\)

            string  imageFile   = string.Empty;
            long    length      = 0;
            bool    found       = false;

            for (int i = 0; i < _searchExtensions.Length; i++)
            {
                FileInfo[] filesFound = _directoryInfo.GetFiles(searchName + _searchExtensions[i]);

                if (filesFound.Length > 0)
                {
                    for (int j = 0; j < filesFound.Length; j++)
                    {
                        if (filesFound[j].Length > length)
                        {
                            length = filesFound[j].Length;
                            imageFile = filesFound[j].FullName;
                            found = true;
                        }
                    }
                }
            }
            if (found) _tagImage = Image.FromFile(imageFile);
            return found;
        }

        /// <summary>
        /// Adds the specified metadata to the specified media file.
        /// </summary>
        /// <param name="fileName">The path and name of the media file to which the metadata should be added.</param>
        /// <param name="title">The title of the media.</param>
        public int SetMetadata(string fileName, string title)
        {
            if (title == null)
            {
                _base._lastError = HResult.E_INVALIDARG;
                return (int)_base._lastError;
            }
            return SetMetadata(fileName, title, null, null, null, null, 0, null, 0, null);
        }

        /// <summary>
        /// Adds the specified metadata to the specified media file.
        /// </summary>
        /// <param name="fileName">The path and name of the media file to which the metadata should be added.</param>
        /// <param name="title">The title of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="albumArtist">The name(s) of the primary artist(s) of the media. Use the value null to leave this item unchanged.</param>
        /// /// <param name="artist">The name(s) of the artist(s) of the media. Use the value null to leave this item unchanged.</param>
        public int SetMetadata(string fileName, string title, string albumArtist, string artist)
        {
            if (title == null && albumArtist == null && artist == null)
            {
                _base._lastError = HResult.E_INVALIDARG;
                return (int)_base._lastError;
            }
            return SetMetadata(fileName, title, null, albumArtist, artist, null, 0, null, 0, null);
        }

        /// <summary>
        /// Adds the specified metadata to the specified media file.
        /// </summary>
        /// <param name="fileName">The path and name of the media file to which the metadata should be added.</param>
        /// <param name="title">The title of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="subTitle">The subtitle of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="albumArtist">The name(s) of the primary artist(s) of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="artist">The name(s) of the artist(s) of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="albumTitle">The album title of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="trackNumber">The track number of the media. Use the value 0 (zero) to leave this item unchanged.</param>
        /// <param name="genre">The genre of the media. Use the value null to leave this item unchanged.</param>
        /// <param name="year">The year the media was published. Use the value 0 (zero) to leave this item unchanged.</param>
        /// <param name="comment">The comment attached to the media. Use the value null to leave this item unchanged.</param>
        public int SetMetadata(string fileName, string title, string subTitle, string albumArtist, string artist, string albumTitle, int trackNumber, string genre, int year, string comment)
        {
            Guid            iid     = typeof(IPropertyStore).GUID;
            IPropertyStore  store   = null;

            if (string.IsNullOrWhiteSpace(fileName)) _base._lastError = HResult.ERROR_INVALID_NAME;
            else if (!File.Exists(fileName)) _base._lastError = HResult.ERROR_FILE_NOT_FOUND;
            else
            {
                try
                {
                    _base._lastError = MFExtern.SHGetPropertyStoreFromParsingName(fileName, IntPtr.Zero, GETPROPERTYSTOREFLAGS.GPS_READWRITE, ref iid, out store);
                    if (_base._lastError == NO_ERROR)
                    {
                        if (title != null) // allow spaces to wipe out
                        {
                            PropVariant prop = new PropVariant(title);
                            store.SetValue(PropertyKeys.PKEY_Title, prop);
                            prop.Dispose();
                        }

                        if (subTitle != null)
                        {
                            PropVariant prop = new PropVariant(subTitle);
                            store.SetValue(PropertyKeys.PKEY_Media_SubTitle, prop);
                            prop.Dispose();
                        }

                        if (albumArtist != null)
                        {
                            PropVariant prop = new PropVariant(albumArtist);
                            store.SetValue(PropertyKeys.PKEY_Music_AlbumArtist, prop);
                            prop.Dispose();
                        }

                        if (artist != null)
                        {
                            PropVariant prop = new PropVariant(artist);
                            store.SetValue(PropertyKeys.PKEY_Music_Artist, prop);
                            prop.Dispose();
                        }

                        if (albumTitle != null)
                        {
                            PropVariant prop = new PropVariant(albumTitle);
                            store.SetValue(PropertyKeys.PKEY_Music_AlbumTitle, prop);
                            prop.Dispose();
                        }

                        if (trackNumber > 0)
                        {
                            PropVariant prop = new PropVariant(trackNumber);
                            store.SetValue(PropertyKeys.PKEY_Music_TrackNumber , prop);
                            prop.Dispose();
                        }

                        if (genre != null)
                        {
                            PropVariant prop = new PropVariant(genre);
                            store.SetValue(PropertyKeys.PKEY_Music_Genre, prop);
                            prop.Dispose();
                        }

                        if (year > 0) // whatever you want
                        {
                            PropVariant prop = new PropVariant(year);
                            store.SetValue(PropertyKeys.PKEY_Media_Year, prop);
                            prop.Dispose();
                        }

                        if (comment != null)
                        {
                            PropVariant prop = new PropVariant(comment);
                            store.SetValue(PropertyKeys.PKEY_Comment, prop);
                            prop.Dispose();
                        }

                        store.Commit();
                    }
                }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }

                if (store != null) Marshal.ReleaseComObject(store);
            }
            return (int)_base._lastError;
        }

    }

    #endregion

    #region Chapters Class

    /// <summary>
    /// A class that is used to group together the Chapters methods and properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public sealed class Chapters : HideObjectMembers
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        #region Fields (Chapters Class)

        private const int       NO_ERROR                = 0;
        private const string    NO_TITLE_INDICATOR      = "#";
        private const int       CHAPTERS_FILE_MAX_SIZE  = 10240;
        private const string    ROOT_ATOM_TYPES         = "ftyp,moov,mdat,pdin,moof,mfra,stts,stsc,stsz,meta,free,skip";
        private const string    IGNORE_EXTENSIONS       = ".chap.srt.m3u.m3u8.ppl.txt.inf.cfg.exe.dll";

        private Player          _base;

        // Chapters file
        private string          _chapDirectory;
        private string          _chapFileName;
        private static Regex    _parser   = new Regex(@"^(?<start>[0-9:,.]+)(\s*)(-(\s*))?(?<end>[0-9:,.]+)?(\s*)(?<title>.+)", RegexOptions.Compiled);

        private byte[]          MOOV_ATOM = { (byte)'m', (byte)'o', (byte)'o', (byte)'v' };
        private byte[]          TRAK_ATOM = { (byte)'t', (byte)'r', (byte)'a', (byte)'k' };
        private byte[]          TREF_ATOM = { (byte)'t', (byte)'r', (byte)'e', (byte)'f' };
        private byte[]          CHAP_ATOM = { (byte)'c', (byte)'h', (byte)'a', (byte)'p' };
        private byte[]          TKHD_ATOM = { (byte)'t', (byte)'k', (byte)'h', (byte)'d' };
        private byte[]          MDIA_ATOM = { (byte)'m', (byte)'d', (byte)'i', (byte)'a' };
        private byte[]          MINF_ATOM = { (byte)'m', (byte)'i', (byte)'n', (byte)'f' };
        private byte[]          STBL_ATOM = { (byte)'s', (byte)'t', (byte)'b', (byte)'l' };
        private byte[]          STTS_ATOM = { (byte)'s', (byte)'t', (byte)'t', (byte)'s' };
        private byte[]          STCO_ATOM = { (byte)'s', (byte)'t', (byte)'c', (byte)'o' };
        private byte[]          UDTA_ATOM = { (byte)'u', (byte)'d', (byte)'t', (byte)'a' };
        private byte[]          CHPL_ATOM = { (byte)'c', (byte)'h', (byte)'p', (byte)'l' };

        private FileStream      _reader;
        private long            _fileLength;
        private long            _atomEnd;
        private long            _moovStart;
        private long            _moovEnd;
        private byte[]          _buffer;

        #endregion

        internal Chapters(Player player)
        {
            _base = player;
        }

        /* Get Chapters From Media */

        /*
            Thanks to Zeugma440, https://github.com/Zeugma440/atldotnet/wiki/Focus-on-Chapter-metadata
            A great help to defeat the ugly QuickTime chapters beast.
        */

        /// <summary>
        /// Returns chapter information from the playing media file. Supported file formats: .mp4, .m4a, .m4b, .m4v, .mkv, .mka and .webm (and maybe others). This method does not evaluate file extensions but the actual content of files.
        /// </summary>
        /// <param name="chapters_I">When this method returns, contains the chapter information of the media stored in the QuickTime (mp4 types) or Matroska (mkv types) format or null.</param>
        /// <param name="chapters_II">When this method returns, contains the chapter information of the media stored the Nero (mp4 types) format or null.</param>
        public int FromMedia(out MediaChapter[] chapters_I, out MediaChapter[] chapters_II)
        {
            if (_base._fileMode && !_base._imageMode) return FromMedia(_base._fileName, out chapters_I, out chapters_II);

            chapters_I = null;
            chapters_II = null;

            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Returns chapter information from the specified media file. Supported file formats: .mp4, .m4a, .m4b, .m4v, .mkv, .mka and .webm (and maybe others). This method does not evaluate file extensions but the actual content of files.
        /// </summary>
        /// <param name="fileName">The path and name of the media file whose chapter information is to be obtained.</param>
        /// <param name="chapters_I">When this method returns, contains the chapter information of the media file stored in the QuickTime (mp4 types) or Matroska (mkv types) format or null.</param>
        /// <param name="chapters_II">When this method returns, contains the chapter information of the media file stored in the Nero (mp4 types) format or null.</param>
        public int FromMedia(string fileName, out MediaChapter[] chapters_I, out MediaChapter[] chapters_II)
        {
            chapters_I = null;
            chapters_II = null;
            int fileType = 0;    // 0 = none, 1 = mp4, 2 = mkv

            if (string.IsNullOrWhiteSpace(fileName)) _base._lastError = HResult.E_INVALIDARG;
            else if (!File.Exists(fileName)) _base._lastError = HResult.ERROR_FILE_NOT_FOUND;
            else
            {
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                try
                {
                    byte[] buffer = new byte[16];
                    _reader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    if (_reader.Length > 1000)
                    {
                        _reader.Read(buffer, 0, 8);
                        if ((ROOT_ATOM_TYPES.IndexOf(Encoding.ASCII.GetString(new byte[] { buffer[4], buffer[5], buffer[6], buffer[7] }), StringComparison.Ordinal) >= 0))
                        {
                            fileType = 1;
                            _base._lastError = NO_ERROR;
                        }
                        else if (buffer[0] == 0x1A && buffer[1] == 0x45 && buffer[2] == 0xDF && buffer[3] == 0xA3)
                        {
                            fileType = 2;
                            _base._lastError = NO_ERROR;
                        }
                    }
                }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
            }

            if (_base._lastError == NO_ERROR)
            {
                _fileLength = _reader.Length;
                _reader.Position = 0;

                if (fileType == 1)
                {
                    chapters_I = GetQuickTimeChapters();
                    if (_moovStart != 0) chapters_II = GetNeroChapters();
                }
                else //if (fileType == 2)
                {
                    chapters_I = GetMatroskaChapters();
                }
            }

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            return (int)_base._lastError;
        }

        private MediaChapter[] GetQuickTimeChapters()
        {
            MediaChapter[] chapters = null;
            byte[] buffer = new byte[256];

            try
            {
                long index = FindAtom(MOOV_ATOM, 0, _fileLength);
                if (index > 0)
                {
                    bool found = false;

                    _moovStart = index;
                    _moovEnd = _atomEnd;
                    long moovIndex = index;
                    long moovEnd = _atomEnd;

                    long oldIndex;
                    long oldEnd;

                    int trackCounter = 0;
                    int trackNumber = 0;

                    while (!found && index < moovEnd)
                    {
                        oldEnd = _atomEnd;

                        // walk the "moov" atom
                        index = FindAtom(TRAK_ATOM, index, _atomEnd);
                        if (index > 0)
                        {
                            oldIndex = _atomEnd;
                            trackCounter++;

                            // walk the "trak" atom
                            index = FindAtom(TREF_ATOM, index, _atomEnd);
                            if (index > 0)
                            {
                                index = FindAtom(CHAP_ATOM, index, _atomEnd);
                                if (index > 0)
                                {
                                    _reader.Read(buffer, 0, 4);
                                    trackNumber = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                                    found = true;

                                    index = oldIndex;
                                    _reader.Position = index;
                                    _atomEnd = oldEnd;

                                    break; // break while
                                }
                            }
                            index = oldIndex;
                            _reader.Position = index;
                            _atomEnd = oldEnd;
                        }
                        else // no more trak atoms - break not really necessary (?)
                        {
                            break;
                        }
                    }

                    if (found)
                    {
                        // get the chapters track
                        int count = trackNumber - trackCounter;
                        if (count < 0)
                        {
                            count = trackNumber;
                            index = moovIndex;
                            _reader.Position = index;
                            _atomEnd = _moovEnd;
                        }
                        for (int i = 0; i < count && index > 0; i++)
                        {
                            index = FindAtom(TRAK_ATOM, index, _atomEnd);
                            if (i < count - 1)
                            {
                                index = _atomEnd;
                                _reader.Position = index;
                                _atomEnd = _moovEnd;
                            }
                        }

                        if (index > 0)
                        {
                            // walk the "trak" atom
                            oldIndex = index;
                            oldEnd = _atomEnd;
                            index = FindAtom(TKHD_ATOM, index, _atomEnd);
                            if (index > 0)
                            {
                                index = oldIndex;
                                _reader.Position = index;
                                _atomEnd = oldEnd;
                                index = FindAtom(MDIA_ATOM, index, _atomEnd);
                                if (index > 0)
                                {
                                    oldIndex = index;

                                    // get time scale
                                    index += 20;
                                    _reader.Position = index;
                                    _reader.Read(buffer, 0, 4);
                                    int timeScale = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                                    if (timeScale == 0) timeScale = 1;

                                    index = oldIndex;
                                    _reader.Position = index;
                                    index = FindAtom(MINF_ATOM, index, _atomEnd);
                                    if (index > 0)
                                    {
                                        index = FindAtom(STBL_ATOM, index, _atomEnd);
                                        if (index > 0)
                                        {
                                            oldIndex = index;
                                            oldEnd = _atomEnd;
                                            index = FindAtom(STTS_ATOM, index, _atomEnd);
                                            if (index > 0)
                                            {
                                                // get chapter start times (durations)
                                                index += 4;
                                                _reader.Position = index;
                                                _reader.Read(buffer, 0, 4);
                                                int startTimeCounter = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];

                                                if (startTimeCounter > 0)
                                                {
                                                    int chapterCount = 1;
                                                    int startTime = 0;

                                                    List<int> startTimes = new List<int>
                                                    {
                                                        0
                                                    };
                                                    while (startTimeCounter > 1)
                                                    {
                                                        _reader.Read(buffer, 0, 8);
                                                        int sampleCount = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                                                        chapterCount += sampleCount;
                                                        startTime += (buffer[4] << 24 | buffer[5] << 16 | buffer[6] << 8 | buffer[7]) / timeScale;
                                                        for (int i = 1; i <= sampleCount; i++)
                                                        {
                                                            startTimes.Add(i * startTime);
                                                        }
                                                        startTimeCounter -= sampleCount;
                                                    }

                                                    index = oldIndex;
                                                    _reader.Position = index;
                                                    _atomEnd = oldEnd;
                                                    index = FindAtom(STCO_ATOM, index, _atomEnd);
                                                    if (index > 0)
                                                    {
                                                        // get chapter titles
                                                        index += 4;
                                                        _reader.Position = index;
                                                        _reader.Read(buffer, 0, 4);
                                                        int entries = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                                                        if (entries == chapterCount)
                                                        {
                                                            chapters = new MediaChapter[chapterCount];
                                                            for (int i = 0; i < chapterCount; i++)
                                                            {
                                                                _reader.Read(buffer, 0, 4);
                                                                int offset1 = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];

                                                                index = _reader.Position;
                                                                _reader.Position = offset1;

                                                                _reader.Read(buffer, 0, 2);
                                                                int len = buffer[0] << 8 | buffer[1];

                                                                _reader.Read(buffer, 0, len);
                                                                chapters[i] = new MediaChapter
                                                                {
                                                                    _title = new string[] { Encoding.UTF8.GetString(buffer, 0, len) },
                                                                    _startTime = TimeSpan.FromSeconds(startTimes[i])
                                                                };

                                                                _reader.Position = index;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { chapters = null; }

            if (chapters != null)
            {
                for (int i = 1; i < chapters.Length; i++)
                {
                    if (chapters[i - 1]._endTime == TimeSpan.Zero)
                    {
                        chapters[i - 1]._endTime = chapters[i]._startTime;
                    }
                }
            }

            return chapters;
        }

        private MediaChapter[] GetNeroChapters()
        {
            MediaChapter[] chapters = null;
            byte[] buffer = new byte[256];

            long index = _moovStart; // retrieved at GetChapters
            _reader.Position = index;
            long moovEnd = _moovEnd;
            _atomEnd = moovEnd;

            try
            {
                while (index < moovEnd)
                {
                    long oldIndex;
                    long oldEnd = _atomEnd;

                    index = FindAtom(UDTA_ATOM, index, _atomEnd);
                    if (index > 0)
                    {
                        oldIndex = _atomEnd;
                        index = FindAtom(CHPL_ATOM, index, _atomEnd);
                        if (index > 0)
                        {
                            index += 5;
                            _reader.Position = index;
                            _reader.Read(buffer, 0, 4);
                            int count = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                            chapters = new MediaChapter[count];
                            int length;

                            for (int i = 0; i < count; i++)
                            {
                                _reader.Read(buffer, 0, 9);

                                chapters[i] = new MediaChapter
                                {
                                    _startTime = TimeSpan.FromTicks(((long)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]) << 32)
                                    | ((buffer[4] << 24 | buffer[5] << 16 | buffer[6] << 8 | buffer[7]) & 0xffffffff))
                                };
                                length = buffer[8];
                                _reader.Read(buffer, 0, length);
                                chapters[i]._title = new string[] { Encoding.UTF8.GetString(buffer, 0, length) };
                            }
                            break; // chapters found and done
                        }
                        else // chapters not found, check for more udta atoms
                        {
                            index = oldIndex;
                            _reader.Position = index;
                            _atomEnd = oldEnd;
                        }
                    }
                    else // no more udta atoms - chapters not present and done
                    {
                        break;
                    }
                }
            }
            catch { chapters = null; }

            if (chapters != null)
            {
                for (int i = 1; i < chapters.Length; i++)
                {
                    if (chapters[i - 1]._endTime == TimeSpan.Zero)
                    {
                        chapters[i - 1]._endTime = chapters[i]._startTime;
                    }
                }
            }

            return chapters;
        }

        private long FindAtom(byte[] type, long startIndex, long endIndex)
        {
            long index = startIndex;
            long end = endIndex - 8;
            byte[] buffer = new byte[16];

            while (index < end)
            {
                _reader.Read(buffer, 0, 8);
                long atomSize = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
                if (atomSize < 2)
                {
                    if (atomSize == 0) atomSize = _fileLength - index;
                    else // size == 1
                    {
                        _reader.Read(buffer, 8, 8);
                        atomSize = ((long)((buffer[8] << 24) | (buffer[9] << 16) | (buffer[10] << 8) | buffer[11]) << 32)
                                    | ((buffer[12] << 24 | buffer[13] << 16 | buffer[14] << 8 | buffer[15]) & 0xffffffff);
                    }
                }

                if (buffer[4] == type[0] && buffer[5] == type[1] && buffer[6] == type[2] && buffer[7] == type[3])
                {
                    _atomEnd = index + atomSize;
                    return _reader.Position; // found
                }

                index += atomSize;
                _reader.Position = index;
            }
            return 0; // not found
        }

        // don't do: _reader.Position += GetDataSize(); etc.
        private MediaChapter[] GetMatroskaChapters()
        {
            MediaChapter[] mkvChapters = null;
            bool found = false;

            try
            {
                _buffer = new byte[256];

                // "EBML Header"
                int idLength = GetElementID();
                if (idLength == 4 && _buffer[0] == 0x1A && _buffer[1] == 0x45 && _buffer[2] == 0xDF && _buffer[3] == 0xA3)
                {
                    // skip
                    _reader.Position = GetDataSize() + _reader.Position;

                    // "Segment"
                    idLength = GetElementID();
                    if (idLength == 4 && _buffer[0] == 0x18 && _buffer[1] == 0x53 && _buffer[2] == 0x80 && _buffer[3] == 0x67)
                    {
                        GetDataSize();
                        long segmentStart = _reader.Position;

                        // "SeekHead" (Meta Seek Info)
                        idLength = GetElementID();
                        if (idLength == 4 && _buffer[0] == 0x11 && _buffer[1] == 0x4D && _buffer[2] == 0x9B && _buffer[3] == 0x74)
                        {
                            long seekEnd = GetDataSize() + _reader.Position;
                            while (_reader.Position < seekEnd)
                            {
                                // "Seek"
                                _reader.Read(_buffer, 0, 2);
                                if (_buffer[0] == 0x4D && _buffer[1] == 0xBB)
                                {
                                    long nextSeek = GetDataSize() + _reader.Position;

                                    // "SeekId"
                                    _reader.Read(_buffer, 0, 2);
                                    if (_buffer[0] == 0x53 && _buffer[1] == 0xAB)
                                    {
                                        // "Chapters"
                                        if (GetDataSize() == 4)
                                        {
                                            _reader.Read(_buffer, 0, 4);
                                            if (_buffer[0] == 0x10 && _buffer[1] == 0x43 && _buffer[2] == 0xA7 && _buffer[3] == 0x70)
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        _reader.Position = nextSeek;
                                    }
                                    else break;
                                }
                                else break;
                            }

                            if (found)
                            {
                                found = false;

                                // "SeekPosition" of "Chapters"
                                _reader.Read(_buffer, 0, 2);
                                if (_buffer[0] == 0x53 && _buffer[1] == 0xAC)
                                {
                                    // get position of "Chapters"
                                    long dataSize = GetDataSize();
                                    _reader.Read(_buffer, 0, (int)dataSize);

                                    long offset = 0;
                                    for (int i = 0; i < dataSize; i++) offset = (offset << 8) + _buffer[i];
                                    _reader.Position = segmentStart + offset;

                                    // "Chapters"
                                    idLength = GetElementID();
                                    if (idLength == 4 && _buffer[0] == 0x10 && _buffer[1] == 0x43 && _buffer[2] == 0xA7 && _buffer[3] == 0x70)
                                    {
                                        dataSize = GetDataSize();

                                        // "EditionEntry"
                                        _reader.Read(_buffer, 0, 2);
                                        if (_buffer[0] == 0x45 && _buffer[1] == 0xB9)
                                        {
                                            // find first "ChapterAtom"
                                            long chapterEnd = GetDataSize() + _reader.Position;
                                            while (!found && _reader.Position < chapterEnd)
                                            {
                                                idLength = GetElementID();
                                                if (idLength == 1 && _buffer[0] == 0xB6)
                                                {
                                                    _reader.Position--;
                                                    found = true;
                                                }
                                                else dataSize = GetDataSize();
                                            }

                                            if (found)
                                            {
                                                // parse all "ChapterAtom"
                                                List<MediaChapter> chapters = new List<MediaChapter>();
                                                do
                                                {
                                                    idLength = GetElementID();
                                                    if (idLength == 1 && _buffer[0] == 0xB6)
                                                    {
                                                        dataSize = GetDataSize();
                                                        long nextChapter = _reader.Position + dataSize;
                                                        MediaChapter chapter = GetChapter(dataSize);
                                                        if (chapter != null)
                                                        {
                                                            chapters.Add(chapter);
                                                            _reader.Position = nextChapter;
                                                        }
                                                        else found = false;
                                                    }
                                                    else found = false;
                                                }
                                                while (found && _reader.Position < chapterEnd);

                                                if (found) mkvChapters = chapters.ToArray();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            if (mkvChapters != null)
            {
                for (int i = 1; i < mkvChapters.Length; i++)
                {
                    if (mkvChapters[i - 1]._endTime == TimeSpan.Zero)
                    {
                        mkvChapters[i - 1]._endTime = mkvChapters[i]._startTime;
                    }
                }
            }

            return mkvChapters;
        }

        private int GetElementID()
        {
            int length = _reader.ReadByte();

            if ((length & 0x80) != 0) length = 1;
            else if ((length & 0x40) != 0) length = 2;
            else if ((length & 0x20) != 0) length = 3;
            else length = 4;

            _reader.Position--;
            _reader.Read(_buffer, 0, length);

            return length;
        }

        private long GetDataSize()
        {
            byte mask = 0x7F;
            int length = _reader.ReadByte();
            _buffer[0] = (byte)length;

            // length == 1 less than true length
            for (int i = 0; i < 8; i++)
            {
                if ((length & 0x80) != 0)
                {
                    length = i;
                    break;
                }
                length <<= 1;
                mask >>= 1;
            }

            _buffer[0] &= mask;
            long result = _buffer[0];

            if (length > 0)
            {
                _reader.Read(_buffer, 1, length);
                for (int i = 0; i <= length; i++)
                {
                    result = (result << 8) + _buffer[i];
                }
            }
            return result;
        }

        private MediaChapter GetChapter(long length)
        {
            MediaChapter chapter = null;
            List<string> languages = new List<string>();
            List<string> titles = new List<string>();
            long startTime = 0;
            long endTime = 0;
            byte id0, id1;

            try
            {
                long chapterEnd = _reader.Position + length;
                while (_reader.Position < chapterEnd)
                {
                    long idLength = GetElementID();
                    id0 = _buffer[0];
                    long dataSize = GetDataSize();

                    if (idLength == 1 && id0 == 0x80) // chapter display
                    {
                        long displayEnd = _reader.Position + dataSize;
                        while (_reader.Position < displayEnd)
                        {
                            idLength = GetElementID();
                            id0 = _buffer[0]; id1 = _buffer[1];
                            dataSize = GetDataSize();
                            _reader.Read(_buffer, 0, (int)dataSize);

                            if (idLength == 1 && id0 == 0x85)
                            {
                                titles.Add(Encoding.UTF8.GetString(_buffer, 0, (int)dataSize));
                            }
                            else if (idLength == 2 && id0 == 0x43 && id1 == 0x7C)
                            {
                                languages.Add(Encoding.UTF8.GetString(_buffer, 0, (int)dataSize));
                            }
                        }
                    }
                    else
                    {
                        _reader.Read(_buffer, 0, (int)dataSize);
                        if (idLength == 1 && id0 == 0x91) // time start
                        {
                            startTime = 0;
                            for (int i = 0; i < dataSize; i++)
                            {
                                startTime = (startTime << 8) + _buffer[i];
                            }
                        }
                        else if (idLength == 1 && id0 == 0x92) // time end
                        {
                            endTime = 0;
                            for (int i = 0; i < dataSize; i++)
                            {
                                endTime = (endTime << 8) + _buffer[i];
                            }
                        }
                    }
                }
                if (titles.Count > 0 && titles.Count == languages.Count)
                {
                    chapter = new MediaChapter
                    {
                        _title = titles.ToArray(),
                        _language = languages.ToArray(),
                        _startTime = TimeSpan.FromTicks(startTime / 100),
                        _endTime = TimeSpan.FromTicks(endTime / 100)
                    };
                }
            }
            catch { chapter = null; }
            return chapter;
        }


        /* Get Chapters From Text File */

        /// <summary>
        /// Gets or sets the initial directory to search for chapter files with the Player.Chapters.FromFile method (default: string.Empty (the directory of the playing media)).
        /// </summary>
        public string Directory
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_chapDirectory == null) _chapDirectory = string.Empty;
                return _chapDirectory;
            }
            set
            {
                _base._lastError = HResult.E_INVALIDARG;
                if (!string.IsNullOrWhiteSpace(value) && System.IO.Directory.Exists(value))
                {
                    try
                    {
                        _chapDirectory = Path.GetDirectoryName(value);
                        _base._lastError = NO_ERROR;
                    }
                    catch (Exception e)
                    {
                        _chapDirectory = string.Empty;
                        _base._lastError = (HResult)Marshal.GetHRForException(e);
                    }
                }
                else _chapDirectory = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the file name (without path and extension) of the chapters file to search for with the Player.Chapters.FromFile method (default: string.Empty (the file name of the playing media)). Reset to string.Empty after the Player.Chapters.FromFile method is used.
        /// </summary>
        public string FileName
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_chapFileName == null) _chapFileName = string.Empty;
                return _chapFileName;
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                    _chapFileName = value;
                    _base._lastError = NO_ERROR;
                }
                else
                {
                    _chapFileName = string.Empty;
                    _base._lastError = HResult.E_INVALIDARG;
                }
            }
        }

        /// <summary>
        /// Gets the number of chapters in the playing media (applies only to chapters played using the Player.Play method).
        /// </summary>
        public int Count
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._chapterMode ? _base._mediaChapters.Length : 0;
            }
        }

        /// <summary>
        /// Gets or sets the (zero-based) index of the chapter being played (applies only to chapters played using the Player.Play method). 
        /// </summary>
        public int Index
        {
            get
            {
                if (_base._chapterMode)
                {
                    _base._lastError = NO_ERROR;
                    return _base._chapterIndex;
                }
                _base._lastError = HResult.MF_E_NOT_AVAILABLE;
                return 0;
            }
            set
            {
                if (_base._chapterMode)
                {
                    if (value >= 0 && value < _base._mediaChapters.Length)
                    {
                        // set index -1 lower because AV_EndOfMedia first increases index
                        _base._chapterIndex = value - 1;
                        SetChapter();
                    }
                    else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            }
        }

        /// <summary>
        /// Plays the next chapter (applies only to chapters played using the Player.Play method).
        /// </summary>
        public int PlayNext()
        {
            if (_base._chapterMode)
            {
                if (_base._chapterIndex < _base._mediaChapters.Length - 1)
                {
                    // set index -1 lower because AV_EndOfMedia first increases index
                    SetChapter();
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Plays the previous chapter (applies only to chapters played using the Player.Play method).
        /// </summary>
        public int PlayPrevious()
        {
            if (_base._chapterMode)
            {
                if (_base._chapterIndex > 0)
                {
                    // set index -1 lower because AV_EndOfMedia first increases index
                    _base._chapterIndex -= 2;
                    SetChapter();
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Returns the chapter being played or null if not available (applies only to chapters played using the Player.Play method).
        /// </summary>
        public MediaChapter GetChapter()
        {
            MediaChapter chapter = null;

            if (_base._chapterMode)
            {
                int index = _base._chapterIndex;
                if (index >= 0 && index < _base._mediaChapters.Length)
                {
                    chapter = _base._mediaChapters[index];
                    _base._lastError = NO_ERROR;
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;

            return chapter;
        }

        /// <summary>
        /// Returns the chapter with the specified index or null if not available (applies only to chapters played using the Player.Play method).
        /// </summary>
        /// <param name="index">The (zero-based) index of the chapter to be retrieved.</param>
        public MediaChapter GetChapter(int index)
        {
            MediaChapter chapter = null;

            if (_base._chapterMode)
            {
                if (index >= 0 && index < _base._mediaChapters.Length)
                {
                    chapter = _base._mediaChapters[index];
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;

            return chapter;
        }

        /// <summary>
        /// Returns the chapters being played or null if not available (applies only to chapters played using the Player.Play method).
        /// </summary>
        public MediaChapter[] GetChapters()
        {
            MediaChapter[] chapters = null;

            if (_base._chapterMode)
            {
                chapters = _base._mediaChapters;
                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;

            return chapters;
        }

        private void SetChapter()
        {
            _base._repeatChapterCount = 0;
            bool repeat = _base._repeatChapter;
            _base._repeatChapter = false; // needed for logic at AV_EndOfMedia
            _base.AV_EndOfMedia();
            _base._repeatChapter = repeat;

            _base._lastError = NO_ERROR;
        }

        /// <summary>
        /// Returns the chapters for the playing media. The information is obtained from a file with the same name as the playing media file but with the extension ".chap" located in the same folder as the media file or in one of the folders contained therein. See also: Player.Chapters.Directory and Player.Chapters.FileName.
        /// </summary>
        public MediaChapter[] FromFile()
        {
            if (_base._fileMode)
            {
                string path;
                string fileName;

                if (!string.IsNullOrWhiteSpace(_chapFileName))
                {
                    fileName = _chapFileName + Player.CHAPTERS_FILE_EXTENSION;
                    _chapFileName = string.Empty;
                }
                else fileName = Path.Combine(Path.GetFileNameWithoutExtension(_base._fileName), Player.CHAPTERS_FILE_EXTENSION);

                if (!string.IsNullOrWhiteSpace(_chapDirectory)) path = _base.Subtitles_FindFile(fileName, _chapDirectory, 1);
                else path = _base.Subtitles_FindFile(fileName, Path.GetDirectoryName(_base._fileName), 1);

                return GetChaptersFile(path);
            }
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return null;
        }

        /// <summary>
        /// Returns the chapters from the specified chapters file.
        /// </summary>
        /// <param name="fileName">The path and file name of the chapters file.</param>
        public MediaChapter[] FromFile(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                return GetChaptersFile(fileName);
            }

            _base._lastError = HResult.ERROR_INVALID_NAME;
            return null;
        }

        private MediaChapter[] GetChaptersFile(string path)
        {
            MediaChapter[] chapters = null;
            _base._lastError = HResult.MF_E_NOT_AVAILABLE;

            if (path.Length > 0 && File.Exists(path) && new FileInfo(path).Length < CHAPTERS_FILE_MAX_SIZE)
            {
                try
                {
                    string[] lines = File.ReadAllLines(path);
                    int count = lines.Length;
                    if (count > 0)
                    {
                        Match match;
                        bool error      = false;
                        int trueCount   = 0;

                        chapters = new MediaChapter[count];

                        for (int i = 0; i < count && !error; i++)
                        {
                            lines[i] = lines[i].Trim();
                            if (lines[i].Length > 0 && lines[i][0] != '#')
                            {
                                match = _parser.Match(lines[i]);
                                if (match.Success)
                                {
                                    chapters[trueCount] = new MediaChapter();
                                    if (TimeSpan.TryParse(match.Groups["start"].Value, out chapters[trueCount]._startTime))
                                    {
#pragma warning disable CA1806 // Do not ignore method results
                                        if (match.Groups["end"].Value != string.Empty) TimeSpan.TryParse(match.Groups["end"].Value, out chapters[trueCount]._endTime);
#pragma warning restore CA1806 // Do not ignore method results
                                        chapters[trueCount]._title = new string[1];

                                        if (match.Groups["title"].Value != null && match.Groups["title"].Value.Length == 1 && (match.Groups["title"].Value[0] >= '0' && match.Groups["title"].Value[0] <= '9')) chapters[trueCount]._title[0] = NO_TITLE_INDICATOR;
                                        else chapters[trueCount]._title[0] = match.Groups["title"].Value;

                                        if (trueCount > 0 && chapters[trueCount - 1]._endTime == TimeSpan.Zero)
                                        {
                                            chapters[trueCount - 1]._endTime = chapters[trueCount]._startTime;
                                        }

                                        trueCount++;
                                    }
                                    else error = true;
                                }
                                else error = true;
                            }
                        }

                        if (error || trueCount == 0) chapters = null;
                        else
                        {
                            if (trueCount != count) Array.Resize(ref chapters, trueCount);
                            _base._lastError = NO_ERROR;
                        }
                    }
                }
                catch { /* ignored */ }
            }

            return chapters;
        }


		// Get Base Media File

		/// <summary>
		/// Returns the path and file name of the media file (located in the same or parent directory) belonging to the specified chapters file or null if not found.
		/// </summary>
		/// <param name="fileName">The path and file name of the chapters file.</param>
#pragma warning disable CA1822 // Mark members as static
		public string GetMediaFile(string fileName)
#pragma warning restore CA1822 // Mark members as static
		{
            string result = null;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    string mediaName = Path.GetFileNameWithoutExtension(fileName);
                    if (!string.IsNullOrWhiteSpace(mediaName))
                    {
                        mediaName = mediaName.Trim();

                        // search home directory
                        string directory = Path.GetDirectoryName(fileName);
                        result = GetBaseFile(directory, mediaName);

                        // search parent directory
                        if (result == null)
                        {
                            DirectoryInfo parent = System.IO.Directory.GetParent(directory);
                            if (parent.FullName != null) result = GetBaseFile(parent.FullName, mediaName);
                        }
                    }
                }
                catch { /* ignored */ }
            }
            return result;
        }

        private static string GetBaseFile(string directory, string fileName)
        {
            string result = null;
            try
            {
                IEnumerable<string> baseFiles = System.IO.Directory.EnumerateFiles(directory, fileName + ".*");
                foreach (string baseFile in baseFiles)
                {
                    string extension = Path.GetExtension(baseFile);
                    if (IGNORE_EXTENSIONS.IndexOf(extension, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        result = Path.Combine(directory, baseFile);
                        break;
                    }
                }
            }
            catch { /* ignored */ }
            return result;
        }


        /* Save Chapter Information To A File */

        /// <summary>
        /// Saves the specified chapters to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the file. The file extension is set to ".chap".</param>
        /// <param name="chapters">The chapters to save.</param>
        public int ToFile(string fileName, MediaChapter[] chapters)
        {
            return ToFile(fileName, chapters, 0);
        }

        /// <summary>
        /// Saves the specified chapters to the specified file. If the file already exists, it is overwritten.
        /// </summary>
        /// <param name="fileName">The path and file name of the file. The file extension is set to ".chap".</param>
        /// <param name="chapters">The chapters to save.</param>
        /// <param name="titleIndex">The index of the chapter title to save (if multiple languages are used, default: 0).</param>
        public int ToFile(string fileName, MediaChapter[] chapters, int titleIndex)
        {
            if (string.IsNullOrWhiteSpace(fileName) || chapters == null) _base._lastError = HResult.E_INVALIDARG;
            else
            {
                StringBuilder text = new StringBuilder(1024);
                try
                {
                    text.AppendLine("# " + Path.GetFileNameWithoutExtension(fileName));
                    int initLength = text.Length;
                    for (int i = 0; i < chapters.Length; i++)
                    {
                        if (chapters[i] != null)
                        {
                            text.Append(chapters[i]._startTime.ToString(Player.CHAPTERs_TIME_FORMAT));
                            if (chapters[i]._endTime != TimeSpan.Zero) text.Append(" - ").Append(chapters[i]._endTime.ToString(Player.CHAPTERs_TIME_FORMAT));
                            if (chapters[i]._title == null || string.IsNullOrWhiteSpace(chapters[i]._title[0])) text.AppendLine(" #");
                            else
                            {
                                if (titleIndex < 0 || titleIndex >= chapters[i]._title.Length) text.Append(' ').AppendLine(chapters[i]._title[0]);
                                else text.Append(' ').AppendLine(chapters[i]._title[titleIndex]);
                            }
                        }
                    }
                    if (text.Length > initLength)
                    {
                        File.WriteAllText(Path.ChangeExtension(fileName, Player.CHAPTERS_FILE_EXTENSION), text.ToString());
                        _base._lastError = NO_ERROR;
                    }
                    else _base._lastError = HResult.E_INVALIDARG;
                }
                catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                text.Length = 0;
            }
            return (int)_base._lastError;
        }
    }

	#endregion

	#region Images Class

	/// <summary>
	/// A class that is used to group together the Images properties of the ABX.MediaPlayer.Player class.
	/// </summary>
	[CLSCompliant(true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class Images : HideObjectMembers
    {
        #region Fields (Images Class)

        private const int NO_ERROR          = 0;
        private const int MINIMUM_FRAMERATE = 4;
        private const int MAXIMUM_FRAMERATE = 30;
        private const int MINIMUM_DURATION  = 1;
        private const int MAXIMUM_DURATION  = 60;

        private Player _base;

        #endregion

        internal Images(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the player also plays images (default: true).
        /// </summary>
        public bool Enabled
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._imagesEnabled;
            }
            set
            {
                _base._lastError = NO_ERROR;
                _base._imagesEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the frame rate at which images are played. Values from 4 to 30 frames per second (default: 16).
        /// </summary>
        public int FrameRate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._imageFrameRate;
            }
            set
            {
                if (value < MINIMUM_FRAMERATE || value > MAXIMUM_FRAMERATE)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    _base._lastError = NO_ERROR;
                    _base._imageFrameRate = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the duration of image playback. Values from 1 to 60 seconds (default: 5).
        /// </summary>
        public int Duration
        {
            get
            {
                _base._lastError = NO_ERROR;
                return (int)(_base._imageDuration / Player.ONE_SECOND_TICKS);
            }
            set
            {
                if (value < MINIMUM_DURATION || value > MAXIMUM_DURATION)
                {
                    _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else
                {
                    _base._lastError = NO_ERROR;
                    _base._imageDuration = value * Player.ONE_SECOND_TICKS;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether an image is playing (including paused image). Use the Player.Play method to play an image.
        /// </summary>
        public bool Playing
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._imageMode;
            }
        }

        /// <summary>
        /// Updates or restores the video image on the player's display window. For special use only, generally not required.
        /// </summary>
        public int Update()
        {
            if (_base._imageMode && _base.mf_VideoDisplayControl != null)
            {
                _base._lastError = NO_ERROR;
                _base.mf_VideoDisplayControl.RepaintVideo();
            }
            else { _base._lastError = HResult.MF_E_NOT_AVAILABLE; }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the image will be retained on the player's display window after it has finished playing (default: false). Can be used to smooth the transition between images. If set to true, the value must be reset to false when all media playback is complete to clear the display. Same as Player.Display.Hold. See also: Player.Images.HoldClear.
        /// </summary>
        public bool Hold
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._displayHold;
            }
            set
            {
                if (value != _base._displayHold)
                {
                    _base._displayHold = value;
                    if (!value) _base.AV_ClearHold();
                }
                _base._lastError = NO_ERROR;
            }
        }

        /// <summary>
        /// Clears the player's display when the Player.Image.Hold option is enabled and no media is playing. Same as: Player.Display.HoldClear. See also: Player.Image.Hold.
        /// </summary>
        public int HoldClear()
        {
            if (_base._displayHold)
            {
                _base.AV_ClearHold();
                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            return (int)_base._lastError;
        }
    }

    #endregion

    #region Playlist Class

    /// <summary>
    /// A class that is used to group together the Playlist methods of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Playlist : HideObjectMembers
    {
        #region Fields (Playlist Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Playlist(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Returns the path and file names from the specified m3u type playlist file or null if none are found.
        /// </summary>
        /// <param name="playlist">The path and file name of the playlist file. Supported file types are .m3u and .m3u8.</param>
        public string[] Open(string playlist)
        {
            List<string> fileNames = null;

            if (string.IsNullOrWhiteSpace(playlist))
            {
                _base._lastError = HResult.ERROR_INVALID_NAME;
            }
            else
            {
                bool validExtension = false;
                bool m3u8 = false;

                string extension = Path.GetExtension(playlist);
                if (extension.Length == 0)
                {
                    playlist += ".m3u";
                    validExtension = true;
                }
                else if (string.Equals(extension, ".m3u", StringComparison.OrdinalIgnoreCase) || (string.Equals(extension, ".ppl", StringComparison.OrdinalIgnoreCase)))
                {
                    validExtension = true;
                }
                else if (string.Equals(extension, ".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    validExtension = true;
                    m3u8 = true;
                }

                if (validExtension)
                {
                    if (File.Exists(playlist))
                    {
                        StreamReader file = null;
                        string playListPath = Path.GetDirectoryName(playlist);
                        string line;

                        fileNames = new List<string>(16);

                        try
                        {
                            if (m3u8) file = new StreamReader(playlist, Encoding.UTF8);
                            else file = new StreamReader(playlist); // something wrong with Encoding.Default?
							while ((line = file.ReadLine()) != null)
							{
								line = line.TrimStart();
								// skip if line is empty, #extm3u, #extinf info or # comment
								//if (line != string.Empty && line[0] != '#')
								if (line.Length > 0 && line[0] != '#')
								{
									// get absolute path...
									if (line[1] != ':' && !line.Contains(@"://") && !line.Contains(@":\\")) fileNames.Add(Path.GetFullPath(Path.Combine(playListPath, line)));
									else fileNames.Add(line);
								}
							}
							_base._lastError = NO_ERROR;
                        }
                        catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }

                        if (file != null) file.Close();
                    }
                    else _base._lastError = HResult.ERROR_FILE_NOT_FOUND;
                }
                else _base._lastError = HResult.ERROR_INVALID_NAME;
            }

            if (fileNames == null || fileNames.Count == 0) return null;
            return fileNames.ToArray();
        }

        /// <summary>
        /// Saves the specified file names as an m3u type playlist file. If the playlist file already exists, it is overwritten.
        /// </summary>
        /// <param name="playlist">The path and file name of the playlist. Supported file types are .m3u and .m3u8.</param>
        /// <param name="fileNames">The media file names to save to the specified playlist file.</param>
        /// <param name="relativePaths">A value that indicates whether to use relative (to the playlist) paths with the saved file names.</param>
        public int Save(string playlist, string[] fileNames, bool relativePaths)
        {
            if (string.IsNullOrWhiteSpace(playlist) || fileNames == null || fileNames.Length == 0)
            {
                _base._lastError = HResult.E_INVALIDARG;
            }
            else
            {
                bool validExtension = false;
                bool m3u8 = false;

                string extension = Path.GetExtension(playlist);
                if (extension.Length == 0)
                {
                    playlist += ".m3u";
                    validExtension = true;
                }
                else if (string.Equals(extension, ".m3u", StringComparison.OrdinalIgnoreCase) || (string.Equals(extension, ".ppl", StringComparison.OrdinalIgnoreCase)))
                {
                    validExtension = true;
                }
                else if (string.Equals(extension, ".m3u8", StringComparison.OrdinalIgnoreCase))
                {
                    validExtension = true;
                    m3u8 = true;
                }

                if (validExtension)
                {
                    if (relativePaths)
                    {
                        int count = fileNames.Length;
                        string[] relPaths = new string[count];
                        for (int i = 0; i < count; ++i)
                        {
                            relPaths[i] = GetRelativePath(playlist, fileNames[i]);
                        }
                        fileNames = relPaths;
                    }
                    try
                    {
                        if (m3u8) File.WriteAllLines(playlist, fileNames, Encoding.UTF8);
                        else File.WriteAllLines(playlist, fileNames);
                        _base._lastError = NO_ERROR;
                    }
                    catch (Exception e) { _base._lastError = (HResult)Marshal.GetHRForException(e); }
                }
                else _base._lastError = HResult.ERROR_INVALID_NAME;
            }
            return (int)_base._lastError;
        }

        // Taken from: https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        // Thanks Dave!
        private static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrWhiteSpace(toPath)) return string.Empty;

            Uri fromUri, toUri;

            try
            {
                fromUri = new Uri(fromPath);
                toUri = new Uri(toPath);

                if (fromUri.Scheme != toUri.Scheme) return toPath;
            }
            catch { return toPath; }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }

    }

    #endregion

    #region Has Class

    /// <summary>
    /// A class that is used to group together the Has (active components) properties of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Has : HideObjectMembers
    {
        #region Fields (Has Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Has(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains audio.
        /// </summary>
        public bool Audio
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasAudio;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains audio but no video.
        /// </summary>
        public bool AudioOnly
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_base._hasVideo) return false;
                return _base._hasAudio;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has active peak level information of the audio output.
        /// </summary>
        public bool AudioPeakLevels
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.pm_HasPeakMeter;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has active peak level information of the audio input.
        /// </summary>
        public bool AudioInputLevels
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.pm_HasInputMeter;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains video.
        /// </summary>
        public bool Video
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasVideo;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media contains video but no audio.
        /// </summary>
        public bool VideoOnly
        {
            get
            {
                _base._lastError = NO_ERROR;
                if (_base._hasAudio) return false;
                return _base._hasVideo;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has a display overlay.
        /// </summary>
        public bool Overlay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlay;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player's display overlay is shown.
        /// </summary>
        public bool OverlayShown
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasOverlayShown;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has a video bitmap overlay.
        /// </summary>
        public bool BitmapOverlay
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasImageOverlay;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has one or more display clones.
        /// </summary>
        public bool DisplayClones
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.dc_HasDisplayClones;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the playing media has active subtitles.
        /// </summary>
        public bool Subtitles
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.st_HasSubtitles;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has one or more taskbar progress indicators.
        /// </summary>
        public bool TaskbarProgress
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasTaskbarProgress;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player has a custom shaped display window.
        /// </summary>
        public bool DisplayShape
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._hasDisplayShape;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing media (including paused media). See also: Player.Media.SourceType.
        /// </summary>
        public bool Media
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._playing;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing an image (including paused image). See also: Player.Media.SourceType.
        /// </summary>
        public bool Image
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._imageMode;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing a webcam (including paused webcam). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool Webcam
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._webcamMode;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing a webcam with audio (including paused webcam with audio). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool WebcamWithAudio
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._webcamAggregated;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing a live stream (including paused live stream). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool LiveStream
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._liveStreamMode;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing an online file (not live) stream (including paused online file stream). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool FileStream
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._fileStreamMode;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing an online (file or live) stream (including paused online stream). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool OnlineStream
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._liveStreamMode | _base._fileStreamMode;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the player is playing an audio input device (without a webcam - including paused audio input). See also: Player.Media.SourceType and Player.Media.SourceCategory.
        /// </summary>
        public bool AudioInput
        {
            get
            {
                _base._lastError = NO_ERROR;
                return ((_base._webcamMode && _base._webcamAggregated) || _base._micMode);
            }
        }

		/// <summary>
		/// Gets a value that indicates whether the player has a display window.
		/// </summary>
		public bool Display
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._hasDisplay;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the player is playing media chapters (with the Player.Play(media, chapters) method).
		/// </summary>
		public bool Chapters
		{
			get
			{
				_base._lastError = NO_ERROR;
				return _base._chapterMode;
			}
		}

        /// <summary>
		/// Gets a value indicating whether the devices (webcam and/or audio input) played by the player are recorded.
		/// </summary>
		public bool Recording
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._recording;
            }
        }
    }

	#endregion

	#region Speed Class

	/// <summary>
	/// A class that is used to group together the playback Speed methods and properties of the ABX.MediaPlayer.Player class.
	/// </summary>
	[CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Speed : HideObjectMembers
    {
        #region Fields (Speed Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Speed(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets or sets a value that indicates the speed at which media is played by the player (default: 1.0 (normal speed)). The setting is adjusted by the player if media cannot be played at the set speed.
        /// </summary>
        public float Rate
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._speed;
            }
            set
            {
                if (value < _base.mf_SpeedMinimum || value > _base.mf_SpeedMaximum)
                {
                    _base._lastError = HResult.MF_E_UNSUPPORTED_RATE;
                }
                else _base.AV_SetSpeed(value, true);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether stream thinning (displaying fewer video frames) should be used when playing media. This option can be used to increase the maximum playback speed of media (use together with Player.Audio.Cut for very fast playback speeds) (default: false).
        /// </summary>
        public bool Boost
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base._speedBoost;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base._speedBoost)
                {
                    _base._speedBoost = value;

                    if (_base.mf_RateControl != null)
                    {
                        ((IMFRateSupport)_base.mf_RateControl).GetFastestRate(MFRateDirection.Forward, _base._speedBoost, out _base.mf_SpeedMaximum);
                        ((IMFRateSupport)_base.mf_RateControl).GetSlowestRate(MFRateDirection.Forward, _base._speedBoost, out _base.mf_SpeedMinimum);

                        if (_base._speed != Player.DEFAULT_SPEED)
                        {
                            _base.mf_RateControl.GetRate(_base._speedBoost, out float trueSpeed);
                            if (_base._speed != trueSpeed)
                            {
                                _base._speed = trueSpeed == 0 ? 1 : trueSpeed;
                                _base.mf_Speed = _base._speed;
                                if (_base._speedSlider != null) _base.SpeedSlider_ValueToSlider(_base._speed);
                                _base._mediaSpeedChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates the minimum speed at which the playing media can be played by the player.
        /// </summary>
        public float Minimum
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.mf_SpeedMinimum;
            }
        }

        /// <summary>
        /// Gets a value that indicates the maximum speed at which the playing media can be played by the player.
        /// </summary>
        public float Maximum
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.mf_SpeedMaximum;
            }
        }
    }

	#endregion

	#region Network Class

	/// <summary>
	/// A class that is used to group together the Network methods and properties of the ABX.MediaPlayer.Player class.
	/// </summary>
	[CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Network : HideObjectMembers
    {
        #region Fields (Network Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion
        
        internal Network(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets statistical network information about the playing media (HTTP not supported).
        /// </summary>
        /// <param name="statistics">Specifies the statistics to be obtained, for example NetworkStatistics.DownloadProgress.</param>
        public long GetStatistics(NetworkStatistics statistics)
        {
            long result = 0;

            if (_base.mf_MediaSession == null) _base._lastError = HResult.MF_E_NOT_AVAILABLE;
            else
            {
                try
                {
                    _base._lastError = MFExtern.MFGetService(_base.mf_MediaSession, MFServices.MFNETSOURCE_STATISTICS_SERVICE, typeof(IPropertyStore).GUID, out object store);
                    if (_base._lastError == NO_ERROR)
                    {
                        PropertyKeys.PKEY_NetSource_Statistics.pID = (int)statistics;
                        PropVariant data = new PropVariant();

                        ((IPropertyStore)store).GetValue(PropertyKeys.PKEY_NetSource_Statistics, data);
                        if (statistics == NetworkStatistics.BytesReceived || statistics == NetworkStatistics.SeekRangeStart || statistics == NetworkStatistics.SeekRangeEnd) result = data.GetLong();
                        else result = data.GetInt();

                        data.Dispose();
                        Marshal.ReleaseComObject(store);
                    }
                }
                catch { /* ignored */ }
            }
            return result;
        }

        /// <summary>
        /// Gets or sets a value that indicates the player's low latency network mode (default: false). This property also applies to local media playback and is reset (to false) when media ends playing.
        /// </summary>
        public bool LowLatency
        {
            get
            {
                _base._lastError = NO_ERROR;
                return _base.mf_LowLatency;
            }
            set
            {
                _base._lastError = NO_ERROR;
                if (value != _base.mf_LowLatency)
                {
                    _base.mf_LowLatency = value;
                    if (_base._playing) _base.AV_UpdateTopology();
                }
            }
        }

        /*
        bool mf_NetCredentialEnabled;
        bool _hasNetCredentialManager;
        bool _hasHetCredentialCache;
        IMFNetCredentialManager mf_NetCredentialManager;
        IMFNetCredentialCache mf_NetCredentialCache;
        //IMFNetCredential        mf_NetCredential;

        /// <summary>
        /// Gets or sets a value that indicates whether the player's network authentication is enabled (this player only - default: false). Please note: Credentials might be sent in clear text.
        /// </summary>
        public bool EnableAuthentication
        {
            get
            {
                _base._lastError = NO_ERROR;
                return mf_NetCredentialEnabled;
            }
            set
            {
                _base._lastError = NO_ERROR;
                mf_NetCredentialEnabled = value;
            }
        }

        /// <summary>
        /// Adds the specified credentials to the network authentication credential cache used by all players (with authentication enabled) in this assembly. See also: Player.Network.EnableAuthentication.
        /// </summary>
        /// <param name="url">The URL for which the authentication is needed.</param>
        /// <param name="realm">The realm for the authentication.</param>
        /// <param name="userName">The user name for the authentication.</param>
        /// <param name="password">The password for the authentication.</param>
        public int AddCredentials(string url, string realm, string userName, string password)
        {
            return AddCredentials(url, realm, userName, password, AuthenticationFlags.None, true);
        }

        /// <summary>
        /// Adds the specified credentials to the network authentication credential cache used by all players (with authentication enabled) in this assembly. See also: Player.Network.EnableAuthentication.
        /// </summary>
        /// <param name="url">The URL for which the authentication is needed.</param>
        /// <param name="realm">The realm for the authentication.</param>
        /// <param name="userName">The user name for the authentication.</param>
        /// <param name="password">The password for the authentication.</param>
        /// <param name="flags">Specifies how the credentials will be used (default: AuthenticationFlags.None).</param>
        /// <param name="allowClearText">Specifies whether the credentials can be sent over the network in plain text when requested (default: true).</param>
        public int AddCredentials(string url, string realm, string userName, string password, AuthenticationFlags flags, bool allowClearText)
        {


            return 0;
        }

        public void GetCredentials(string url, string realm, out string userName, out string password, out AuthenticationFlags flags, out bool allowClearText)
        {
            userName = string.Empty;
            password = string.Empty;
            flags = AuthenticationFlags.None;
            allowClearText = true;
        }

        public void RemoveCredentials(string url, string realm)
        {
        }

        /// <summary>
        /// Net credentials test method.
        /// </summary>
        public void CredentialsTest()
        {
            HResult result = MFExtern.MFCreateCredentialCache(out mf_NetCredentialCache);
            if (result == NO_ERROR)
            {
                IMFNetCredential _netCredential;
                MFNetCredentialRequirements _netCredentialRequirements;
                result = mf_NetCredentialCache.GetCredential("https://CodeProject.com", string.Empty, MFNetAuthenticationFlags.None, out _netCredential, out _netCredentialRequirements);
                MessageBox.Show("Create Credential: " + _base.GetErrorString((int)result));

                byte[] userName = Encoding.ASCII.GetBytes("Peter Vegter" + '\0');
                byte[] password = Encoding.ASCII.GetBytes("Peter1234" + '\0');

                result = _netCredential.SetUser(userName, userName.Length, false);
                if (result == NO_ERROR) result = _netCredential.SetPassword(password, password.Length, false);
                MessageBox.Show("Set User: " + _base.GetErrorString((int)result));

                if (result == NO_ERROR)
                {
                    IMFNetCredential testCredential;
                    result = mf_NetCredentialCache.GetCredential("https://CodeProject.com", string.Empty, MFNetAuthenticationFlags.None, out testCredential, out _netCredentialRequirements);
                    MessageBox.Show("Get Credential: " + _base.GetErrorString((int)result));
                    if (result == NO_ERROR)
                    {
                        byte[] testName = new byte[256];
                        MFInt testData = new MFInt(256);
                        bool testEncrypted = false;
                        result = testCredential.GetUser(testName, testData, testEncrypted);
                        MessageBox.Show("Get User: " + _base.GetErrorString((int)result));

                        if (result == NO_ERROR)
                        {
                            if (testName == null)
                            {
                                MessageBox.Show("User Name: is null");
                            }
                            else
                            {
                                //string testUserName = testName.ToString();
                                string testUserName = Encoding.UTF8.GetString(testName, 0, testName.Length);
                                MessageBox.Show("User Name: " + testUserName);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Create CredentialCache: " + _base.GetErrorString((int)result));
            }

        }
        */
    }

    #endregion

    #region DragAndDrop Class

    /// <summary>
    /// A class that is used to group together the Drag-and-drop methods of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DragAndDrop : HideObjectMembers
    {
        #region Fields (DragAndDrop Class)

        private const int           NO_ERROR        = 0;

        private Player              _base;
        private IDropTargetHelper   _dropHelper;
        private Win32Point          _location;

        #endregion

        internal DragAndDrop(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Enables drag-and-drop ghost images to be displayed when dragged over the specified form. Can be used when no actual drag-and-drop functionality is implemented (otherwise use Player.DragAndDrop.DragEnter etc.).
        /// </summary>
        /// <param name="form">The form on which drag-and-drop ghost images should be enabled.</param>
        public int Add(Form form)
        {
            if (form != null)
            {
                form.AllowDrop = true;

                form.DragEnter += Form_DragEnter;
                form.DragOver += Form_DragOver;
                form.DragLeave += Form_DragLeave;
                form.DragDrop += Form_DragDrop;

                _base._lastError =NO_ERROR;
            }
            else _base._lastError = HResult.E_INVALIDARG;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Disables drag-and-drop ghost images to be displayed when dragged over the specified form (if enabled with Player.DragAndDrop.Add).
        /// </summary>
        /// <param name="form">The form on which drag-and-drop ghost images should be disabled.</param>
        public int Remove(Form form)
        {
            if (form != null)
            {
                form.AllowDrop = false;

                form.DragEnter -= Form_DragEnter;
                form.DragOver -= Form_DragOver;
                form.DragLeave -= Form_DragLeave;
                form.DragDrop -= Form_DragDrop;

                _base._lastError = NO_ERROR;
            }
            else _base._lastError = HResult.E_INVALIDARG;
            return (int)_base._lastError;
        }

        private void Form_DragEnter(object sender, DragEventArgs e) { DragEnter(e); }
        private void Form_DragOver(object sender, DragEventArgs e) { DragOver(e); }
        private void Form_DragLeave(object sender, EventArgs e) { DragLeave(); }
        private void Form_DragDrop(object sender, DragEventArgs e) { DragDrop(e); }

        /// <summary>
        /// Enables drag-and-drop ghost images to be displayed when dragged over a control (for example, a form). Add to the OnDragEnter method (or DragEnter event handler) of a control. Always use all 4 Player.DragAndDrop methods: DragEnter, DragOver, DragLeave and DragDrop.
        /// </summary>
        /// <param name="e">The event arguments received with the OnDragEnter method or the DragEnter event handler (just pass them to this method).</param>
        public void DragEnter(DragEventArgs e)
        {
            if (e != null)
            {
                _location.x = e.X; _location.y = e.Y;
                _dropHelper = (IDropTargetHelper)new DragDropHelper();
                _dropHelper.DragEnter(IntPtr.Zero, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref _location, (int)e.Effect);

                //_base._lastError = NO_ERROR;
            }
            //else _base._lastError = HResult.E_INVALIDARG;
            //return (int)_base._lastError;
        }

        /// <summary>
        /// Enables drag-and-drop ghost images to be displayed when dragged over a control (for example, a form). Add to the OnDragOver method (or DragOver event handler) of a control. Always use all 4 Player.DragAndDrop methods: DragEnter, DragOver, DragLeave and DragDrop.
        /// </summary>
        /// <param name="e">The event arguments received with the OnDragOver method or the DragOver event handler (just pass them to this method).</param>
        public void DragOver(DragEventArgs e)
        {
            if (e != null)
            {
                _location.x = e.X; _location.y = e.Y;
                _dropHelper.DragOver(ref _location, (int)e.Effect);

                //_base._lastError = NO_ERROR;
            }
            //else _base._lastError = HResult.E_INVALIDARG;
            //return (int)_base._lastError;
        }

        /// <summary>
        /// Enables drag-and-drop ghost images to be displayed when dragged over a control (for example, a form). Add to the OnDragLeave method (or DragLeave event handler) of a control. Always use all 4 Player.DragAndDrop methods: DragEnter, DragOver, DragLeave and DragDrop.
        /// </summary>
        public void DragLeave()
        {
            _dropHelper.DragLeave();
            Marshal.ReleaseComObject(_dropHelper);
            _dropHelper = null;

            //_base._lastError = NO_ERROR;
            //return (int)_base._lastError;
        }

        /// <summary>
        /// Enables drag-and-drop ghost images to be displayed when dragged over a control (for example, a form). Add to the OnDragDrop method (or DragDrop event handler) of a control. Always use all 4 Player.DragAndDrop methods: DragEnter, DragOver, DragLeave and DragDrop.
        /// </summary>
        /// <param name="e">The event arguments received with the OnDragDrop method or the DragDrop event handler (just pass them to this method).</param>
        public void DragDrop(DragEventArgs e)
        {
            if (e != null)
            {
                _location.x = e.X; _location.y = e.Y;
                _dropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref _location, (int)e.Effect);
                Marshal.ReleaseComObject(_dropHelper);
                _dropHelper = null;

                //_base._lastError = NO_ERROR;
            }
            //else _base._lastError = HResult.E_INVALIDARG;
            //return (int)_base._lastError;
        }

    }

    #endregion

    #region Events Class

    /// <summary>
    /// A class that is used to group together the Events of the ABX.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Events : HideObjectMembers
    {
        #region Fields (Events Class)

        private const int   NO_ERROR = 0;

        private Player      _base;

        #endregion

        internal Events(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Occurs when media playback has ended.
        /// </summary>
        public event EventHandler<EndedEventArgs> MediaEnded
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaEnded += value;
            }
            remove { _base._mediaEnded -= value; }
        }

        /// <summary>
        /// Occurs when media playback has ended, just before the Player.Events.MediaEnded event occurs.
        /// </summary>
        public event EventHandler<EndedEventArgs> MediaEndedNotice
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaEndedNotice += value;
            }
            remove { _base._mediaEndedNotice -= value; }
        }

        /// <summary>
        /// Occurs when the player's media repeat setting has changed.
        /// </summary>
        public event EventHandler MediaRepeatChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaRepeatChanged += value;
            }
            remove { _base._mediaRepeatChanged -= value; }
        }

        /// <summary>
        /// Occurs when media playback has ended and is repeated.
        /// </summary>
        public event EventHandler MediaRepeated
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaRepeated += value;
            }
            remove { _base._mediaRepeated -= value; }
        }

        /// <summary>
        /// Occurs when the player's chapter repeat setting has changed.
        /// </summary>
        public event EventHandler MediaChapterRepeatChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaChapterRepeatChanged += value;
            }
            remove { _base._mediaChapterRepeatChanged -= value; }
        }

        /// <summary>
        /// Occurs when a chapter playback has ended and is repeated (applies only to chapters played using the Player.Play method).
        /// </summary>
        public event EventHandler MediaChapterRepeated
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaChapterRepeated += value;
            }
            remove { _base._mediaChapterRepeated -= value; }
        }

        /// <summary>
        /// Occurs when media starts playing.
        /// </summary>
        public event EventHandler MediaStarted
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaStarted += value;
            }
            remove { _base._mediaStarted -= value; }
        }

        /// <summary>
        /// Occurs when a new chapter has started playing (applies only to chapters played using the Player.Play method).
        /// </summary>
        public event EventHandler<ChapterStartedEventArgs> MediaChapterStarted
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaChapterStarted += value;
            }
            remove { _base._mediaChapterStarted -= value; }
        }

        /// <summary>
        /// Occurs when the player's pause mode is activated (playing media is paused) or deactivated (paused media is resumed).
        /// </summary>
        public event EventHandler MediaPausedChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaPausedChanged += value;
            }
            remove { _base._mediaPausedChanged -= value; }
        }

        /// <summary>
        /// Occurs when the playback position of the playing media has changed.
        /// </summary>
        public event EventHandler<PositionEventArgs> MediaPositionChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaPositionChanged += value;
            }
            remove { _base._mediaPositionChanged -= value; }
        }

        /// <summary>
        /// Occurs when the start or stop time of the playing media has changed.
        /// </summary>
        public event EventHandler MediaStartStopTimeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaStartStopTimeChanged += value;
            }
            remove { _base._mediaStartStopTimeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's display window has changed.
        /// </summary>
        public event EventHandler MediaDisplayChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaDisplayChanged += value;
            }
            remove { _base._mediaDisplayChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's displaymode has changed.
        /// </summary>
        public event EventHandler MediaDisplayModeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaDisplayModeChanged += value;
            }
            remove { _base._mediaDisplayModeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the shape of the player's display window has changed.
        /// </summary>
        public event EventHandler MediaDisplayShapeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaDisplayShapeChanged += value;
            }
            remove { _base._mediaDisplayShapeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's full screen mode has changed.
        /// </summary>
        public event EventHandler MediaFullScreenChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaFullScreenChanged += value;
            }
            remove { _base._mediaFullScreenChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's full screen display mode has changed.
        /// </summary>
        public event EventHandler MediaFullScreenModeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaFullScreenModeChanged += value;
            }
            remove { _base._mediaFullScreenModeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's audio volume has changed.
        /// </summary>
        public event EventHandler MediaAudioVolumeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaAudioVolumeChanged += value;
            }
            remove { _base._mediaAudioVolumeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's audio balance has changed.
        /// </summary>
        public event EventHandler MediaAudioBalanceChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaAudioBalanceChanged += value;
            }
            remove { _base._mediaAudioBalanceChanged -= value; }
        }

        ///// <summary>
        ///// Occurs when the player's audio enabled setting has changed.
        ///// </summary>
        //public event EventHandler MediaAudioEnabledChanged
        //{
        //    add
        //    {
        //        _base._lastError = NO_ERROR;
        //        _base._mediaAudioMuteChanged += value;
        //    }
        //    remove { _base._mediaAudioMuteChanged -= value; }
        //}

        /// <summary>
        /// Occurs when the player's audio mute setting has changed.
        /// </summary>
        public event EventHandler MediaAudioMuteChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaAudioMuteChanged += value;
            }
            remove { _base._mediaAudioMuteChanged -= value; }
        }

        /// <summary>
        /// Occurs when the videobounds of the video on the player's display window have changed (by using the player's VideoBounds, Video Zoom, etc. options).
        /// </summary>
        public event EventHandler MediaVideoBoundsChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoBoundsChanged += value;
            }
            remove { _base._mediaVideoBoundsChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's video aspect ratio has changed.
        /// </summary>
        public event EventHandler MediaVideoAspectRatioChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoAspectRatioChanged += value;
            }
            remove { _base._mediaVideoAspectRatioChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's video View3D setting has changed.
        /// </summary>
        public event EventHandler MediaVideo3DViewChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoView3DChanged += value;
            }
            remove { _base._mediaVideoView3DChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's video crop setting has changed (by using the Video Crop option).
        /// </summary>
        public event EventHandler MediaVideoCropChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoCropChanged += value;
            }
            remove { _base._mediaVideoCropChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's playback speed setting has changed.
        /// </summary>
        public event EventHandler MediaSpeedChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaSpeedChanged += value;
            }
            remove { _base._mediaSpeedChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's display overlay has changed.
        /// </summary>
        public event EventHandler MediaOverlayChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaOverlayChanged += value;
            }
            remove { _base._mediaOverlayChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's display overlay mode setting has changed.
        /// </summary>
        public event EventHandler MediaOverlayModeChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaOverlayModeChanged += value;
            }
            remove { _base._mediaOverlayModeChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's display overlay hold setting has changed.
        /// </summary>
        public event EventHandler MediaOverlayHoldChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaOverlayHoldChanged += value;
            }
            remove { _base._mediaOverlayHoldChanged -= value; }
        }

        /// <summary>
        /// Occurs when the active state of the player's display overlay has changed.
        /// </summary>
        public event EventHandler MediaOverlayActiveChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaOverlayActiveChanged += value;
            }
            remove { _base._mediaOverlayActiveChanged -= value; }
        }

        /// <summary>
        /// Occurs when a display clone is added or removed from the player.
        /// </summary>
        public event EventHandler MediaDisplayClonesChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaDisplayClonesChanged += value;
            }
            remove { _base._mediaDisplayClonesChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's audio output peak level has changed. Device changes are handled automatically by the player.
        /// </summary>
        public event EventHandler<PeakLevelEventArgs> MediaAudioOutputLevelChanged
        {
            add
            {
                if (_base.PeakMeter_Open(_base._audioDevice, false))
                {
                    if (_base._outputLevelArgs == null) _base._outputLevelArgs = new PeakLevelEventArgs();
                    _base._mediaAudioOutputLevelChanged += value;
                    _base._lastError = NO_ERROR;
                    _base.StartMainTimerCheck();
                }
                else _base._lastError = HResult.ERROR_NOT_READY;
            }
            remove
            {
                if (_base.pm_HasPeakMeter)
                {
                    _base._outputLevelArgs._channelCount = _base.pm_PeakMeterChannelCount;
                    _base._outputLevelArgs._masterPeakValue = -1;
                    _base._outputLevelArgs._channelsValues = _base.pm_PeakMeterValuesStop;
                    value(this, _base._outputLevelArgs);

                    _base._mediaAudioOutputLevelChanged -= value;
                    if (_base._mediaAudioOutputLevelChanged == null)
                    {
                        _base.PeakMeter_Close();
                        _base.StopMainTimerCheck();
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when the player's audio input peak level has changed.
        /// </summary>
        public event EventHandler<PeakLevelEventArgs> MediaAudioInputLevelChanged
        {
            add
            {
                _base._lastError = NO_ERROR;

                if (_base._inputLevelArgs == null) _base._inputLevelArgs = new PeakLevelEventArgs();
                _base._mediaAudioInputLevelChanged += value;

                if (_base._micDevice == null)
                {
                    _base.pm_InputMeterPending = true;
                }
                else if (_base.InputMeter_Open(_base._micDevice, false))
                {
                    _base._mediaAudioInputLevelChanged += value;
                    _base.pm_InputMeterPending = false;
                    _base.StartMainTimerCheck();
                }
            }
            remove
            {
                _base._mediaAudioInputLevelChanged -= value;
                if (_base._mediaAudioInputLevelChanged == null)
                {
                    if (_base.pm_HasInputMeter)
                    {
                        _base.InputMeter_Close();
                        _base.StopMainTimerCheck();
                    }
                    _base.pm_InputMeterPending = false;
                }
            }
        }

        /// <summary>
        /// Occurs when the player's current subtitle has changed.
        /// </summary>
        public event EventHandler<SubtitleEventArgs> MediaSubtitleChanged
        {
            add
            {
                _base._lastError = NO_ERROR;

                if (_base.st_SubtitleChangedArgs == null) _base.st_SubtitleChangedArgs = new SubtitleEventArgs();
                _base._mediaSubtitleChanged += value;

                if (!_base.st_SubtitlesEnabled)
                {
                    _base.st_SubtitlesEnabled = true;
                    if (!_base.st_HasSubtitles && _base._playing)
                    {
                        _base.Subtitles_Start(true);
                        _base.StartMainTimerCheck();
                    }
                }
            }
            remove
            {
                _base._mediaSubtitleChanged -= value;
                if (_base._mediaSubtitleChanged == null)
                {
                    if (_base.st_HasSubtitles)
                    {
                        _base.st_SubtitleOn = false; // prevent 'no title' event firing
                        _base.Subtitles_Stop();
                    }
                    _base.st_SubtitlesEnabled = false;
                    _base.StopMainTimerCheck();
                }
            }
        }

        /// <summary>
        /// Occurs when the active video track of the playing media has changed.
        /// </summary>
        public event EventHandler MediaVideoTrackChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoTrackChanged += value;
            }
            remove { _base._mediaVideoTrackChanged -= value; }
        }

        /// <summary>
        /// Occurs when the active audio track of the playing media has changed.
        /// </summary>
        public event EventHandler MediaAudioTrackChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaAudioTrackChanged += value;
            }
            remove { _base._mediaAudioTrackChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's audio output device has changed. The player handles all changes to the audio output devices. This event could be used to update the user interface of an application.
        /// </summary>
        public event EventHandler MediaAudioDeviceChanged
        {
            add
            {
                _base._mediaAudioDeviceChanged += value;
                _base._lastError = NO_ERROR;
                _base.StartSystemDevicesChangedHandlerCheck();
            }
            remove
            {
                if (_base._mediaAudioDeviceChanged != null)
                {
                    _base._mediaAudioDeviceChanged -= value;
                    _base.StopSystemDevicesChangedHandlerCheck();
                }
            }
        }

        /// <summary>
        /// Occurs when the audio output devices of the system have changed. The player handles all changes to the system audio output devices. This event could be used to update the user interface of an application.
        /// </summary>
        public event EventHandler<SystemAudioDevicesEventArgs> MediaSystemAudioDevicesChanged
        {
            add
            {
                if (Player.AudioDevicesClientOpen())
                {
                    if (_base._mediaSystemAudioDevicesChanged == null) Player._masterSystemAudioDevicesChanged += _base.SystemAudioDevicesChanged;
                    _base._mediaSystemAudioDevicesChanged += value;
                    _base._lastError = NO_ERROR;
                }
                else _base._lastError = HResult.ERROR_NOT_READY;
            }
            remove
            {
                _base._mediaSystemAudioDevicesChanged -= value;
                if (_base._mediaSystemAudioDevicesChanged == null)
                {
                    Player._masterSystemAudioDevicesChanged -= _base.SystemAudioDevicesChanged;
                    _base.StopSystemDevicesChangedHandlerCheck();
                }
            }
        }

        /// <summary>
        /// Occurs when a video color attribute (for example, brightness) of the player has changed.
        /// </summary>
        public event EventHandler<VideoColorEventArgs> MediaVideoColorChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaVideoColorChanged += value;
            }
            remove { _base._mediaVideoColorChanged -= value; }
        }

        /// <summary>
        /// Occurs when the player's audio input device has changed.
        /// </summary>
        public event EventHandler MediaAudioInputDeviceChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaAudioInputDeviceChanged += value;
            }
            remove { _base._mediaAudioInputDeviceChanged -= value; }
        }

        /// <summary>
        /// Occurs when the format of the playing webcam has changed.
        /// </summary>
        public event EventHandler MediaWebcamFormatChanged
        {
            add
            {
                _base._lastError = NO_ERROR;
                _base._mediaWebcamFormatChanged += value;
            }
            remove { _base._mediaWebcamFormatChanged -= value; }
        }

        ///// <summary>
        ///// Occurs when the player's video recorder starts recording.
        ///// </summary>
        //public event EventHandler MediaRecorderStarted
        //{
        //    add
        //    {
        //        _base._lastError = NO_ERROR;
        //        _base._mediaRecorderStarted += value;
        //    }
        //    remove { _base._mediaRecorderStarted -= value; }
        //}

        ///// <summary>
        ///// Occurs when the player's video recorder stops recording.
        ///// </summary>
        //public event EventHandler MediaRecorderStopped
        //{
        //    add
        //    {
        //        _base._lastError = NO_ERROR;
        //        _base._mediaRecorderStopped += value;
        //    }
        //    remove { _base._mediaRecorderStopped -= value; }
        //}

        ///// <summary>
        ///// Occurs when the player's video recorder pause mode is activated (recording is paused) or deactivated (recording is resumed).
        ///// </summary>
        //public event EventHandler MediaRecorderPausedChanged
        //{
        //    add
        //    {
        //        _base._lastError = NO_ERROR;
        //        _base._mediaRecorderPausedChanged += value;
        //    }
        //    remove { _base._mediaRecorderPausedChanged -= value; }
        //}

        /// <summary>
        /// Occurs when the player's device recorder has started recording.
        /// </summary>
        public event EventHandler MediaDeviceRecorderStarted
		{
			add
			{
				_base._lastError = NO_ERROR;
				_base._mediaDeviceRecorderStarted += value;
			}
			remove { _base._mediaDeviceRecorderStarted -= value; }
		}

        /// <summary>
        /// Occurs when the player's device recorder has stopped recording.
        /// </summary>
        public event EventHandler MediaDeviceRecorderStopped
		{
			add
			{
				_base._lastError = NO_ERROR;
				_base._mediaDeviceRecorderStopped += value;
			}
			remove { _base._mediaDeviceRecorderStopped -= value; }
		}
	}

    #endregion

    /*

    // ******************************** Video Recorder Class

    /// <summary>
    /// Represents a video recorder that can be used to store video images in a file.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class VideoRecorder : HideObjectMembers
    {
        #region Fields (Video Recorder Class)

        // Constants
        private const int       BUSY_TIME_OUT           = 1000;
        private const int       DEFAULT_FRAME_RATE      = 15;
        private const bool      DEFAULT_SHOW_OVERLAY    = true;

        // Base Player
        private Player          _base;
        private bool            _basePaused;

        // Recorder Type
        private bool            _webcam;

        // Timer
        private volatile System.Threading.Timer
                                _timer;
        private volatile int    _timerInterval;
        private volatile bool   _timerRestart;
        private volatile bool   _timerBusy;

        // Buffers
        private IMFMediaBuffer  _mediaBuffer;
        private Bitmap          _bitmapBuffer;
        private int             _bytesPerPixel  = 4;
        private Guid             _mediaType     = MFMediaType.RGB32;

        // Sink Writer
        private string          _fileName;
        private IMFSinkWriter   _sinkWriter;

        // Recorder Settings
        private int             _frameRate              = DEFAULT_FRAME_RATE;
        private bool            _showOverlay            = DEFAULT_SHOW_OVERLAY;

        internal bool           _recording;
        private bool            _paused;

        #endregion


        internal VideoRecorder(Player player, bool webCam)
        {
            _base = player;
            _webcam = webCam;
        }

        public int Start(string fileName)
        {
            return (int)_base._lastError;
        }

        public int Start(string fileName, int frameRate)
        {
            return (int)_base._lastError;
        }

        public int Start(string fileName, bool showOverlay)
        {
            return (int)_base._lastError;
        }

        public int Start(string fileName, int frameRate, bool showOverlay)
        {
            return (int)_base._lastError;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Pause()
        {
            return (int)_base._lastError;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Resume()
        {
            return (int)_base._lastError;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Stop()
        {
            if (_recording)
            {
                _base.Events.MediaPausedChanged -= BasePlayer_MediaPausedChanged;
                _base.Events.MediaEndedNotice -= BasePlayer_MediaEndedNotice;

                // stop timer etc.

                _recording = false;
                _paused = false;
            }
            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets the number of video frames to record per second. Values from 0 to 60 (default: 15).
        /// </summary>
        public int FrameRate
        {
            get
            {
                return _frameRate;
            }
            set
            {
            }

        }

        /// <summary>
        /// Gets or sets a value that indicates whether display overlays (if any) are also recorded (default: true).
        /// </summary>
        public bool ShowOverlay
        {
            get
            {
                return _showOverlay;
            }
            set
            {
            }
        }

        // ********

        private int StartRecording()
        {
            // create timer etc.

            _base.Events.MediaPausedChanged += BasePlayer_MediaPausedChanged;
            _base.Events.MediaEndedNotice += BasePlayer_MediaEndedNotice;

            return (int)_base._lastError;
        }

        private void BasePlayer_MediaPausedChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BasePlayer_MediaEndedNotice(object sender, EndedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private HResult Recorder_Init()
        {
            // check status
            if (!_base._hasVideo || (_webcam && !_base._webcamMode)) return HResult.MF_E_NOT_AVAILABLE;

            HResult result = NO_ERROR;

            // create even size
            int width = _base._videoBoundsClip.Width % 2 == 0 ? _base._videoBoundsClip.Width : _base._videoBoundsClip.Width - 1;
            int height = _base._videoBoundsClip.Height % 2 == 0 ? _base._videoBoundsClip.Height : _base._videoBoundsClip.Height - 1;
            if (_showOverlay && (_base._hasOverlay && _base._overlay.Visible))
            {
                width = _base._display.DisplayRectangle.Width;
                height = _base._display.DisplayRectangle.Height;
                if (width == height)
                {
                    if (width != _base._display.DisplayRectangle.Width) width += 2;
                    else if (height != _base._display.DisplayRectangle.Height) height += 2;
                    else height -= 2;
                }
            }
            else if (width == height)
            {
                if (width != _base._videoBoundsClip.Width) width += 2;
                else if (height != _base._videoBoundsClip.Height) height += 2;
                else height -= 2;
            }

            // set pixel depth (TO DO calculate)
            //_bytesPerPixel = 4;
            //_mediaType = MFMediaType.RGB32;

            // create sinkwriter
            _sinkWriter = CreateSinkWriter(_fileName, width, height);
            if (_base._lastError == NO_ERROR && _sinkWriter != null)
            {
                // create buffer
                _bitmapBuffer = new Bitmap(width, height);



                // add pixel depth

                //result = MFExtern.MFCreateMemoryBuffer(bufferSize, out wr_MediaBuffer);
                if (result == NO_ERROR)
                {
                }
            }


            return result;
        }

        #region Recorder - Timer Start / Stop

        private void Recorder_StartTimer()
        {
            if (_timer == null)
            {
                _timerRestart = true;
                _timer = new System.Threading.Timer(Recorder_Callback, null, 0, System.Threading.Timeout.Infinite);
            }
        }

        private void Recorder_StopTimer()
        {
            if (_timer != null)
            {
                _timerRestart = false;
                _timer.Dispose();
                _timer = null;

                int timeOut = BUSY_TIME_OUT;
                while (_timerBusy && --timeOut > 0)
                {
                    System.Threading.Thread.Sleep(1);
                    Application.DoEvents();
                }
            }
        }

        #endregion

        private IMFSinkWriter CreateSinkWriter(string fileName, int width, int height)
        {
            const int VIDEO_BIT_RATE = 800000;

            int streamIndex = 0;

            HResult result = MFExtern.MFCreateSinkWriterFromURL(fileName, null, null, out IMFSinkWriter sinkWriter);
            if (result == NO_ERROR)
            {
                result = MFExtern.MFCreateMediaType(out IMFMediaType mediaTypeOut);
                if (result == NO_ERROR)
                {
                    result = mediaTypeOut.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
                    if (result == NO_ERROR)
                    {
                        // if changing the encoder also change the file extension (IMAGES_FILE_EXTENSION)
                        result = mediaTypeOut.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.H264);
                        if (result == NO_ERROR)
                        {
                            result = mediaTypeOut.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, VIDEO_BIT_RATE);
                            if (result == NO_ERROR)
                            {
                                result = mediaTypeOut.SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, 2); // 2 = Progressive
                                if (result == NO_ERROR)
                                {
                                    result = MFExtern.MFSetAttributeSize(mediaTypeOut, MFAttributesClsid.MF_MT_FRAME_SIZE, width, height);
                                    if (result == NO_ERROR)
                                    {
                                        result = MFExtern.MFSetAttributeRatio(mediaTypeOut, MFAttributesClsid.MF_MT_FRAME_RATE, _frameRate, 1);
                                        if (result == NO_ERROR)
                                        {
                                            result = MFExtern.MFSetAttributeRatio(mediaTypeOut, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
                                            if (result == NO_ERROR)
                                            {
                                                result = sinkWriter.AddStream(mediaTypeOut, out streamIndex);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (mediaTypeOut != null) Marshal.ReleaseComObject(mediaTypeOut);
            }

            if (result == NO_ERROR)
            {
                result = MFExtern.MFCreateMediaType(out IMFMediaType mediaTypeIn);
                if (result == NO_ERROR)
                {
                    result = mediaTypeIn.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
                    if (result == NO_ERROR)
                    {
                        result = mediaTypeIn.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, _mediaType); // MFMediaType.RGB32);
                        if (result == NO_ERROR)
                        {
                            result = mediaTypeIn.SetUINT32(MFAttributesClsid.MF_MT_INTERLACE_MODE, 2); // 2 = Progressive
                            if (result == NO_ERROR)
                            {
                                result = MFExtern.MFSetAttributeSize(mediaTypeIn, MFAttributesClsid.MF_MT_FRAME_SIZE, width, height);
                                if (result == NO_ERROR)
                                {
                                    result = MFExtern.MFSetAttributeRatio(mediaTypeIn, MFAttributesClsid.MF_MT_FRAME_RATE, _frameRate, 1);
                                    if (result == NO_ERROR)
                                    {
                                        result = MFExtern.MFSetAttributeRatio(mediaTypeIn, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
                                        if (result == NO_ERROR)
                                        {
                                            result = sinkWriter.SetInputMediaType(streamIndex, mediaTypeIn, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (mediaTypeIn != null) Marshal.ReleaseComObject(mediaTypeIn);
            }

            if (result != NO_ERROR && sinkWriter != null)
            {
                Marshal.ReleaseComObject(sinkWriter);
                sinkWriter = null;
            }

            _base._lastError = result;
            return sinkWriter;
        }

        private void Recorder_Callback(object state)
        {
            if (!_timerBusy && _timerRestart)
            {
                IntPtr destHdc = IntPtr.Zero;
                Graphics destGraphics = null;
                Rectangle sourceRect = _base._videoBoundsClip;

                Graphics sourceGraphics = null;
                IntPtr sourceHdc = IntPtr.Zero;

                _timerBusy = true;

                try
                {
                    destGraphics = Graphics.FromImage(_bitmapBuffer); destHdc = destGraphics.GetHdc();
                    sourceGraphics = _base._display.CreateGraphics(); sourceHdc = sourceGraphics.GetHdc();

                    if (_showOverlay && (_base._hasOverlay && _base._overlay.Visible))
                    {
                        if (_base._overlayMode == OverlayMode.Display) sourceRect = _base._display.DisplayRectangle; // with overlay - same size as display

                        // copy display to buffer
                        SafeNativeMethods.StretchBlt(destHdc, 0, 0, _bitmapBuffer.Width, _bitmapBuffer.Height, sourceHdc, sourceRect.Left, sourceRect.Top, sourceRect.Width, sourceRect.Height, SafeNativeMethods.SRCCOPY_U);
                        sourceGraphics.ReleaseHdc(sourceHdc); sourceGraphics.Dispose(); sourceGraphics = null;

                        // copy overlay to buffer - transparent + opacity
                        sourceGraphics = _base._overlay.CreateGraphics(); sourceHdc = sourceGraphics.GetHdc();
                        if (_base._overlay.Opacity == 1 || _base._overlayBlend == OverlayBlend.None)
                        {
                            SafeNativeMethods.TransparentBlt(destHdc, 0, 0, _bitmapBuffer.Width, _bitmapBuffer.Height, sourceHdc, 0, 0, _base._overlay.Width, _base._overlay.Height, ColorTranslator.ToWin32(_base._overlay.TransparencyKey));
                        }
                        else
                        {
                            _base._blendFunction.SourceConstantAlpha = (byte)(_base._overlay.Opacity * 0xFF);
                            SafeNativeMethods.AlphaBlend(destHdc, 0, 0, _bitmapBuffer.Width, _bitmapBuffer.Height, sourceHdc, 0, 0, _base._overlay.Width, _base._overlay.Height, _base._blendFunction);
                        }
                    }
                    else
                    {
                        // copy display to buffer
                        SafeNativeMethods.StretchBlt(destHdc, 0, 0, _bitmapBuffer.Width, _bitmapBuffer.Height, sourceHdc, sourceRect.Left, sourceRect.Top, sourceRect.Width, sourceRect.Height, SafeNativeMethods.SRCCOPY_U);
                    }

                    sourceGraphics.ReleaseHdc(sourceHdc); sourceGraphics.Dispose(); sourceGraphics = null;
                    destGraphics.ReleaseHdc(destHdc); destGraphics.Dispose(); destGraphics = null;

                    //Recorder_WriteImageFrame(IMFSinkWriter sinkWriter, Bitmap image)
                }
                catch
                {
                    if (sourceGraphics != null)
                    {
                        if (sourceHdc != IntPtr.Zero) sourceGraphics.ReleaseHdc(sourceHdc);
                        sourceGraphics.Dispose();
                    }
                    if (destGraphics != null)
                    {
                        if (destHdc != IntPtr.Zero) destGraphics.ReleaseHdc(destHdc);
                        destGraphics.Dispose();
                    }

                }
            }

            if (_timerRestart) _timer.Change(_timerInterval, System.Threading.Timeout.Infinite);
            _timerBusy = false;
        }

        internal void Recorder_WriteImageFrame(IMFSinkWriter sinkWriter, Bitmap image)
        {
            HResult result = NO_ERROR;

            System.Drawing.Imaging.BitmapData bmpData = null;
            try { bmpData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, image.PixelFormat); }
            catch (Exception e) { result = (HResult)Marshal.GetHRForException(e); }

            if (result == NO_ERROR)
            {
                int cbWidth = 0;// _imageBytesPerPixel * image.Width;
                int cbBuffer = cbWidth * image.Height;
                result = MFExtern.MFCreateMemoryBuffer(cbBuffer, out IMFMediaBuffer buffer);

                if (result == NO_ERROR)
                {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = buffer.Lock(out IntPtr data, out int maxLength, out int currentLength);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                    if (result == NO_ERROR)
                    {
                        //result = MFExtern.MFCopyImage(data, cbWidth, bmpData.Scan0, cbWidth, cbWidth, image.Height);
                        result = MFExtern.MFCopyImage(data, cbWidth, bmpData.Scan0 + ((image.Height - 1) * cbWidth), -cbWidth, cbWidth, image.Height);
                        buffer.Unlock();

                        if (result == NO_ERROR)
                        {
                            buffer.SetCurrentLength(cbBuffer);

                            result = MFExtern.MFCreateSample(out IMFSample sample);
                            if (result == NO_ERROR)
                            {
                                result = sample.AddBuffer(buffer);
                                if (result == NO_ERROR)
                                {
                                    result = sample.SetSampleTime(0);
                                    if (result == NO_ERROR)
                                    {
                                        result = sample.SetSampleDuration(0);// _imageDuration);
                                        if (result == NO_ERROR)
                                        {
                                            sinkWriter.WriteSample(0, sample);
                                        }
                                    }
                                }
                                Marshal.ReleaseComObject(sample);
                            }
                        }
                    }
                    Marshal.ReleaseComObject(buffer);
                }
                image.UnlockBits(bmpData);
            }
        }

        internal void Recorder_WriteImageFram2()
        {
            HResult result = NO_ERROR;

            System.Drawing.Imaging.BitmapData bmpData = null;
            try { bmpData = _bitmapBuffer.LockBits(new Rectangle(0, 0, _bitmapBuffer.Width, _bitmapBuffer.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, _bitmapBuffer.PixelFormat); }
            catch (Exception e) { result = (HResult)Marshal.GetHRForException(e); }

            if (result == NO_ERROR)
            {
                int cbWidth = _bytesPerPixel * _bitmapBuffer.Width;
                int cbBuffer = cbWidth * _bitmapBuffer.Height;
                result = MFExtern.MFCreateMemoryBuffer(cbBuffer, out IMFMediaBuffer buffer);

                if (result == NO_ERROR)
                {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = buffer.Lock(out IntPtr data, out int maxLength, out int currentLength);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

                    if (result == NO_ERROR)
                    {
                        //result = MFExtern.MFCopyImage(data, cbWidth, bmpData.Scan0, cbWidth, cbWidth, image.Height);
                        result = MFExtern.MFCopyImage(data, cbWidth, bmpData.Scan0 + ((_bitmapBuffer.Height - 1) * cbWidth), -cbWidth, cbWidth, _bitmapBuffer.Height);
                        buffer.Unlock();

                        if (result == NO_ERROR)
                        {
                            buffer.SetCurrentLength(cbBuffer);

                            result = MFExtern.MFCreateSample(out IMFSample sample);
                            if (result == NO_ERROR)
                            {
                                result = sample.AddBuffer(buffer);
                                if (result == NO_ERROR)
                                {
                                    result = sample.SetSampleTime(0);
                                    if (result == NO_ERROR)
                                    {
                                        result = sample.SetSampleDuration(0);// _imageDuration);
                                        if (result == NO_ERROR)
                                        {
                                            _sinkWriter.WriteSample(0, sample);
                                        }
                                    }
                                }
                                Marshal.ReleaseComObject(sample);
                            }
                        }
                    }
                    Marshal.ReleaseComObject(buffer);
                }
                _bitmapBuffer.UnlockBits(bmpData);
            }
        }
    }
    */

}

