﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright © 2007-2015 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Newtonsoft.Json;
using ShareX.HelpersLib;
using ShareX.ImageEffectsLib;
using ShareX.IndexerLib;
using ShareX.ScreenCaptureLib;
using ShareX.UploadersLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;

namespace ShareX
{
    public class TaskSettings
    {
        public string Description = string.Empty;

        public HotkeyType Job = HotkeyType.None;

        public bool UseDefaultAfterCaptureJob = true;
        public AfterCaptureTasks AfterCaptureJob = AfterCaptureTasks.CopyImageToClipboard | AfterCaptureTasks.SaveImageToFile | AfterCaptureTasks.UploadImageToHost;

        public bool UseDefaultAfterUploadJob = true;
        public AfterUploadTasks AfterUploadJob = AfterUploadTasks.CopyURLToClipboard;

        public bool UseDefaultDestinations = true;
        public ImageDestination ImageDestination = ImageDestination.Imgur;
        public FileDestination ImageFileDestination = FileDestination.Dropbox;
        public TextDestination TextDestination = TextDestination.Pastebin;
        public FileDestination TextFileDestination = FileDestination.Dropbox;
        public FileDestination FileDestination = FileDestination.Dropbox;
        public UrlShortenerType URLShortenerDestination = UrlShortenerType.BITLY;
        public URLSharingServices URLSharingServiceDestination = URLSharingServices.Twitter;

        public bool OverrideFTP = false;
        public int FTPIndex = 0;

        public bool OverrideCustomUploader = false;
        public int CustomUploaderIndex = 0;

        public bool UseDefaultGeneralSettings = true;
        public TaskSettingsGeneral GeneralSettings = new TaskSettingsGeneral();

        public bool UseDefaultImageSettings = true;
        public TaskSettingsImage ImageSettings = new TaskSettingsImage();

        public bool UseDefaultCaptureSettings = true;
        public TaskSettingsCapture CaptureSettings = new TaskSettingsCapture();

        public bool UseDefaultUploadSettings = true;
        public TaskSettingsUpload UploadSettings = new TaskSettingsUpload();

        public bool UseDefaultActions = true;
        public List<ExternalProgram> ExternalPrograms = new List<ExternalProgram>();

        public bool UseDefaultIndexerSettings = true;
        public IndexerSettings IndexerSettings = new IndexerSettings();

        public bool UseDefaultAdvancedSettings = true;
        public TaskSettingsAdvanced AdvancedSettings = new TaskSettingsAdvanced();

        public bool WatchFolderEnabled = false;
        public List<WatchFolderSettings> WatchFolderList = new List<WatchFolderSettings>();

        [JsonIgnore]
        public TaskSettings TaskSettingsReference { get; private set; }

        [JsonIgnore]
        public TaskSettingsCapture TaskSettingsCaptureReference
        {
            get
            {
                if (UseDefaultCaptureSettings)
                {
                    return Program.DefaultTaskSettings.CaptureSettings;
                }

                return TaskSettingsReference.CaptureSettings;
            }
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Description) ? Description : Job.GetLocalizedDescription();
        }

        public bool IsUsingDefaultSettings
        {
            get
            {
                return UseDefaultAfterCaptureJob && UseDefaultAfterUploadJob && UseDefaultDestinations && !OverrideFTP && !OverrideCustomUploader && UseDefaultGeneralSettings &&
                    UseDefaultImageSettings && UseDefaultCaptureSettings && UseDefaultUploadSettings && UseDefaultActions && UseDefaultIndexerSettings &&
                    UseDefaultAdvancedSettings && !WatchFolderEnabled;
            }
        }

        public static TaskSettings GetDefaultTaskSettings()
        {
            TaskSettings taskSettings = new TaskSettings();
            taskSettings.SetDefaultSettings();
            taskSettings.TaskSettingsReference = Program.DefaultTaskSettings;
            return taskSettings;
        }

        public static TaskSettings GetSafeTaskSettings(TaskSettings taskSettings)
        {
            TaskSettings safeTaskSettings;

            if (taskSettings.IsUsingDefaultSettings && Program.DefaultTaskSettings != null)
            {
                safeTaskSettings = Program.DefaultTaskSettings.Copy();
                safeTaskSettings.Description = taskSettings.Description;
                safeTaskSettings.Job = taskSettings.Job;
            }
            else
            {
                safeTaskSettings = taskSettings.Copy();
                safeTaskSettings.SetDefaultSettings();
            }

            safeTaskSettings.TaskSettingsReference = taskSettings;
            return safeTaskSettings;
        }

        private void SetDefaultSettings()
        {
            if (Program.DefaultTaskSettings != null)
            {
                TaskSettings defaultTaskSettings = Program.DefaultTaskSettings.Copy();

                if (UseDefaultAfterCaptureJob)
                {
                    AfterCaptureJob = defaultTaskSettings.AfterCaptureJob;
                }

                if (UseDefaultAfterUploadJob)
                {
                    AfterUploadJob = defaultTaskSettings.AfterUploadJob;
                }

                if (UseDefaultDestinations)
                {
                    ImageDestination = defaultTaskSettings.ImageDestination;
                    ImageFileDestination = defaultTaskSettings.ImageFileDestination;
                    TextDestination = defaultTaskSettings.TextDestination;
                    TextFileDestination = defaultTaskSettings.TextFileDestination;
                    FileDestination = defaultTaskSettings.FileDestination;
                    URLShortenerDestination = defaultTaskSettings.URLShortenerDestination;
                    URLSharingServiceDestination = defaultTaskSettings.URLSharingServiceDestination;
                }

                if (UseDefaultGeneralSettings)
                {
                    GeneralSettings = defaultTaskSettings.GeneralSettings;
                }

                if (UseDefaultImageSettings)
                {
                    ImageSettings = defaultTaskSettings.ImageSettings;
                }

                if (UseDefaultCaptureSettings)
                {
                    CaptureSettings = defaultTaskSettings.CaptureSettings;
                }

                if (UseDefaultUploadSettings)
                {
                    UploadSettings = defaultTaskSettings.UploadSettings;
                }

                if (UseDefaultActions)
                {
                    ExternalPrograms = defaultTaskSettings.ExternalPrograms;
                }

                if (UseDefaultIndexerSettings)
                {
                    IndexerSettings = defaultTaskSettings.IndexerSettings;
                }

                if (UseDefaultAdvancedSettings)
                {
                    AdvancedSettings = defaultTaskSettings.AdvancedSettings;
                }
            }
        }

        public string CaptureFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(AdvancedSettings.CapturePath))
                {
                    return AdvancedSettings.CapturePath;
                }

                return Program.ScreenshotsFolder;
            }
        }
    }

    public class TaskSettingsGeneral
    {
        public bool PlaySoundAfterCapture = true;
        public bool ShowAfterCaptureTasksForm = false;
        public bool ShowBeforeUploadForm = false;
        public bool PlaySoundAfterUpload = true;
        public PopUpNotificationType PopUpNotification = PopUpNotificationType.ToastNotification;
        public bool ShowAfterUploadForm = false;
        public bool SaveHistory = true;
    }

    public class TaskSettingsImage
    {
        #region Image / General

        public EImageFormat ImageFormat = EImageFormat.PNG;
        public int ImageJPEGQuality = 90;
        public GIFQuality ImageGIFQuality = GIFQuality.Default;
        public int ImageSizeLimit = 1024;
        public EImageFormat ImageFormat2 = EImageFormat.JPEG;
        public FileExistAction FileExistAction = FileExistAction.Ask;

        #endregion Image / General

        #region Image / Effects

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<ImageEffect> ImageEffects = ImageEffectManager.GetDefaultImageEffects();

        public bool ShowImageEffectsWindowAfterCapture = false;
        public bool ImageEffectOnlyRegionCapture = false;

        #endregion Image / Effects

        #region Image / Thumbnail

        public int ThumbnailWidth = 200;
        public int ThumbnailHeight = 0;
        public string ThumbnailName = "-thumbnail";
        public bool ThumbnailCheckSize = false;

        #endregion Image / Thumbnail
    }

    public class TaskSettingsCapture
    {
        #region Capture / General

        public bool ShowCursor = true;
        public bool CaptureTransparent = false;
        public bool CaptureShadow = true;
        public int CaptureShadowOffset = 20;
        public bool CaptureClientArea = false;
        public bool IsDelayScreenshot = false;
        public decimal DelayScreenshot = 2.0m;
        public bool CaptureAutoHideTaskbar = false;
        public Rectangle CaptureCustomRegion = new Rectangle(0, 0, 0, 0);

        #endregion Capture / General

        #region Capture / Region capture

        public SurfaceOptions SurfaceOptions = new SurfaceOptions();

        #endregion Capture / Region capture

        #region Capture / Screen recorder

        public FFmpegOptions FFmpegOptions = new FFmpegOptions();
        public int ScreenRecordFPS = 20;
        public int GIFFPS = 5;
        public ScreenRecordGIFEncoding GIFEncoding = ScreenRecordGIFEncoding.FFmpeg;
        public bool ScreenRecordFixedDuration = false;
        public float ScreenRecordDuration = 3f;
        public bool ScreenRecordAutoStart = true;
        public float ScreenRecordStartDelay = 1f;
        public bool ScreenRecordShowCursor = true;
        public bool RunScreencastCLI = false;
        public int VideoEncoderSelected = 0;

        #endregion Capture / Screen recorder

        #region Capture / Rectangle annotate

        public RectangleAnnotateOptions RectangleAnnotateOptions = new RectangleAnnotateOptions();

        #endregion Capture / Rectangle annotate
    }

    public class TaskSettingsUpload
    {
        #region Upload

        public bool UseCustomTimeZone = false;
        public TimeZoneInfo CustomTimeZone = TimeZoneInfo.Utc;
        public string NameFormatPattern = "%y-%mo-%d_%h-%mi-%s"; // Test: %y %mo %mon %mon2 %d %h %mi %s %ms %w %w2 %pm %rn %ra %width %height %app %ver
        public string NameFormatPatternActiveWindow = "%t_%y-%mo-%d_%h-%mi-%s";
        public bool FileUploadUseNamePattern = false;

        #endregion Upload

        #region Upload / Clipboard upload

        public bool ClipboardUploadURLContents = false;
        public bool ClipboardUploadShortenURL = false;
        public bool ClipboardUploadShareURL = false;
        public bool ClipboardUploadAutoIndexFolder = false;

        #endregion Upload / Clipboard upload
    }

    public class TaskSettingsAdvanced
    {
        [Category("General"), DefaultValue(false), Description("Allow after capture tasks for image files by treating them as images when files are handled during file upload, clipboard file upload, drag && drop file upload, watch folder and other tasks.")]
        public bool ProcessImagesDuringFileUpload { get; set; }

        [Category("General"), DefaultValue(false), Description("Use after capture tasks for clipboard image upload.")]
        public bool ProcessImagesDuringClipboardUpload { get; set; }

        [Category("General"), DefaultValue(false), Description("If task contains upload job then this setting will clear clipboard when task start.")]
        public bool AutoClearClipboard { get; set; }

        [Category("General"), DefaultValue(true), Description("Save text as file for tasks such as clipboard text upload, drag and drop text upload, index folder etc.")]
        public bool TextTaskSaveAsFile { get; set; }

        [Category("Image"), DefaultValue(256), Description("Preferred thumbnail width. 0 means aspect ratio will be used to adjust width according to height.")]
        public int ThumbnailPreferredWidth { get; set; }

        [Category("Image"), DefaultValue(0), Description("Preferred thumbnail height. 0 means aspect ratio will be used to adjust height according to width.")]
        public int ThumbnailPreferredHeight { get; set; }

        [Category("Paths"), Description("Custom capture path takes precedence over path configured in Application configuration.")]
        [Editor(typeof(DirectoryNameEditor), typeof(UITypeEditor))]
        public string CapturePath { get; set; }

        [Category("Upload"), Description("Files with these file extensions will be uploaded using image uploader.")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor,System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public List<string> ImageExtensions { get; set; }

        [Category("Upload"), Description("Files with these file extensions will be uploaded using text uploader.")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor,System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public List<string> TextExtensions { get; set; }

        [Category("After upload"), DefaultValue(false), Description("If result URL starts with \"http://\" then replace it with \"https://\".")]
        public bool ResultForceHTTPS { get; set; }

        [Category("After upload"), DefaultValue("$result"),
        Description("Clipboard content format after uploading. Supported variables: $result, $url, $shorturl, $thumbnailurl, $deletionurl, $filepath, $filename, $filenamenoext, $folderpath, $foldername, $uploadtime and other variables such as %y-%mo-%d etc.")]
        public string ClipboardContentFormat { get; set; }

        [Category("After upload"), DefaultValue("$result"), Description("Balloon tip content format after uploading. Supported variables: $result, $url, $shorturl, $thumbnailurl, $deletionurl, $filepath, $filename, $filenamenoext, $folderpath, $foldername, $uploadtime and other variables such as %y-%mo-%d etc.")]
        public string BalloonTipContentFormat { get; set; }

        [Category("After upload"), DefaultValue("$result"), Description("After upload task \"Open URL\" format. Supported variables: $result, $url, $shorturl, $thumbnailurl, $deletionurl, $filepath, $filename, $filenamenoext, $folderpath, $foldername, $uploadtime and other variables such as %y-%mo-%d etc.")]
        public string OpenURLFormat { get; set; }

        [Category("After upload / Automatic URL Shortener"), DefaultValue(0), Description("Automatically shorten URL if the URL is longer than the specified number of characters. 0 means automatic URL shortening is not active.")]
        public int AutoShortenURLLength { get; set; }

        private float toastWindowDuration;

        [Category("After upload / Notifications"), DefaultValue(3f), Description("Specify how long should toast notification window will stay on screen (in seconds).")]
        public float ToastWindowDuration
        {
            get
            {
                return toastWindowDuration;
            }
            set
            {
                toastWindowDuration = Math.Max(value, 0f);
            }
        }

        [Category("After upload / Notifications"), DefaultValue(ContentAlignment.BottomRight), Description("Specify where should toast notification window appear on the screen.")]
        public ContentAlignment ToastWindowPlacement { get; set; }

        [Category("After upload / Notifications"), DefaultValue(ToastClickAction.OpenUrl), Description("Specify action after toast notification window is left clicked."), TypeConverter(typeof(EnumDescriptionConverter))]
        public ToastClickAction ToastWindowClickAction { get; set; }

        private Size toastWindowSize;

        [Category("After upload / Notifications"), DefaultValue(typeof(Size), "400, 300"), Description("Maximum toast notification window size.")]
        public Size ToastWindowSize
        {
            get
            {
                return toastWindowSize;
            }
            set
            {
                toastWindowSize = new Size(Math.Max(value.Width, 100), Math.Max(value.Height, 100));
            }
        }

        [Category("After upload"), DefaultValue(false), Description("After upload form will be automatically closed after 60 seconds.")]
        public bool AutoCloseAfterUploadForm { get; set; }

        [Category("Interaction"), DefaultValue(false), Description("Disable notifications")]
        public bool DisableNotifications { get; set; }

        [Category("Upload text"), DefaultValue("txt"), Description("File extension when saving text to the local hard disk.")]
        public string TextFileExtension { get; set; }

        [Category("Upload text"), DefaultValue("text"), Description("Text format e.g. csharp, cpp, etc.")]
        public string TextFormat { get; set; }

        [Category("Upload text"), DefaultValue(""), Description("Custom text input. Use %input for text input. Example you can create web page with your text in it."),
        Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string TextCustom { get; set; }

        [Category("Upload text"), DefaultValue(true), Description("HTML encode custom text input.")]
        public bool TextCustomEncodeInput { get; set; }

        [Category("Name pattern"), DefaultValue(100), Description("Maximum name pattern length for file name.")]
        public int NamePatternMaxLength { get; set; }

        [Category("Name pattern"), DefaultValue(50), Description("Maximum name pattern title (%t) length for file name.")]
        public int NamePatternMaxTitleLength { get; set; }

        [Category("Tools"), DefaultValue("$r, $g, $b"), Description("Copy this color format to clipboard after using screen color picker. Formats: $r, $g, $b, $hex, $x, $y")]
        public string ScreenColorPickerFormat { get; set; }

        public TaskSettingsAdvanced()
        {
            this.ApplyDefaultPropertyValues();
            ImageExtensions = Enum.GetNames(typeof(ImageFileExtensions)).ToList();
            TextExtensions = Enum.GetNames(typeof(TextFileExtensions)).ToList();
        }
    }
}