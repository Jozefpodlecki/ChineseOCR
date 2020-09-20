using Common;
using GalaSoft.MvvmLight.CommandWpf;
using Gui.Models;
using Gui.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gui.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly OCRSpaceService _ocrSpaceService;
        private readonly HttpClientService _httpClientService;

        private void RaisePropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand<MouseData> MouseDownCommand { get; set; }
        public RelayCommand<MouseData> MouseMoveCommand { get; set; }

        public RelayCommand OCRScanClickCommand { get; set; }
        public RelayCommand ScreenshotClickCommand { get; set; }
        public RelayCommand UploadClickCommand { get; set; }
        public RelayCommand ClipboardClickCommand { get; set; }
        public RelayCommand ControlPasteCommand { get; set; }
        public RelayCommand ControlOpenCommand { get; set; }
        public RelayCommand<DataObject> DragEnterCommand { get; set; }
        public RelayCommand<DataObject> DragLeaveCommand { get; set; }
        public RelayCommand<DataObject> ImageDropCommand { get; set; }
        public RelayCommand<object> ClipboardChangedCommand { get; set; }
        public string _ocrText { get; set; }
        public string OCRText
        {
            get { return _ocrText; }
            set
            {
                _ocrText = value;
                RaisePropertyChanged(nameof(OCRText));
            }
        }
        public string _translatedText { get; set; }
        public string TranslatedText
        {
            get { return _translatedText; }
            set
            {
                _translatedText = value;
                RaisePropertyChanged(nameof(TranslatedText));
            }
        }
        public bool _ocrScanButtonEnabled { get; set; }
        public bool OCRScanButtonEnabled
        {
            get { return _ocrScanButtonEnabled; }
            set
            {
                _ocrScanButtonEnabled = value;
                RaisePropertyChanged(nameof(OCRScanButtonEnabled));
            }
        }
        public bool ScreenshotButtonEnabled { get; set; }
        public bool UploadButtonEnabled { get; set; }
        public bool _translateButtonEnabled;
        public bool TranslateButtonEnabled
        {
            get { return _translateButtonEnabled; }
            set
            {
                _translateButtonEnabled = value;
                RaisePropertyChanged(nameof(TranslateButtonEnabled));
            }
        }
        public bool IsTranslationLanguageSelectionEnabled { get; set; }

        public bool _clipboardButtonEnabled { get; set; }
        public bool ClipboardButtonEnabled
        { 
            get { return _clipboardButtonEnabled; }
            set {
                _clipboardButtonEnabled = value;
                RaisePropertyChanged(nameof(ClipboardButtonEnabled));
            }
        }

        private BitmapSource _storedImage;
        private BitmapSource _image;

        public BitmapSource ScanImage
        {
            get { return _image; }
            set {
                _image = value;
                RaisePropertyChanged(nameof(ScanImage));
            }
        }


        public CollectionView TranslationLanguages { get; }
        public TranslationLanguage TranslationLanguage { get; set; }

        public MainViewModel(
            HttpClientService httpClientService,
            OCRSpaceService ocrSpaceService)
        {
            var item = new TranslationLanguage
            {
                Id = "English",
                Name = "English"
            };
            var list = new List<TranslationLanguage>()
            {
                item
            };
            TranslationLanguages = new CollectionView(list);
            TranslationLanguage = item;

            _ocrSpaceService = ocrSpaceService;
            _httpClientService = httpClientService;
            MouseDownCommand = new RelayCommand<MouseData>(MouseDown, CanMouseDown);
            MouseMoveCommand = new RelayCommand<MouseData>(MouseMove, CanMouseMove);
            OCRScanClickCommand = new RelayCommand(OCRScanClick, CanOCRScanClick);
            ScreenshotClickCommand = new RelayCommand(ScreenshotClick, CanScreenshotClick);
            UploadClickCommand = new RelayCommand(UploadClick, CanUploadClick);
            ClipboardClickCommand = new RelayCommand(ClipboardClick, () => _clipboardButtonEnabled);
            ControlPasteCommand = new RelayCommand(ControlPaste, () => true);
            ControlOpenCommand = new RelayCommand(ControlOpen, () => true);
            DragEnterCommand = new RelayCommand<DataObject>((dataObject) => {}, CanDragEnter);
            DragLeaveCommand = new RelayCommand<DataObject>((dataObject) => {}, CanDragLeave);
            ImageDropCommand = new RelayCommand<DataObject>(ImageDrop, CanDrop);
            ClipboardChangedCommand = new RelayCommand<object>(ClipboardChanged, (obj) => true);
            OCRScanButtonEnabled = false;
            TranslateButtonEnabled = false;
            UploadButtonEnabled = true;
            ScreenshotButtonEnabled = true;
            IsTranslationLanguageSelectionEnabled = false;
        }

        private void ClipboardChanged(object data)
        {
            
        }

        private bool CanDragLeave(DataObject dataObject)
        {
            Trace.WriteLine("Leave " + DateTime.Now);
            ScanImage = null;
            return false;
        }

        private bool CanDragEnter(DataObject dataObject)
        {
            var formats = dataObject.GetFormats();

            if (dataObject.ContainsImage())
            {
                var bitmapSource = dataObject.GetImage();
                var bitmap = Utils.BitmapSourceToBitmap(bitmapSource);
                Utils.LightenImage(bitmap);
                _storedImage = bitmapSource;
                bitmapSource = Utils.BitmapToBitmapSource(bitmap);
                ScanImage = bitmapSource;
                return true;
            }

            if (dataObject.ContainsFileDropList())
            {
                var stream = dataObject.GetStreamByFileGroupDescriptorW();

                try
                {
                    var image = Image.FromStream(stream);
                    _storedImage = Utils.ImageToBitmapImage(image);
                    var bitmap = new Bitmap(image);
                    Utils.LightenImage(bitmap);
                    ScanImage = Utils.Bitmap2BitmapImage(bitmap);
                    return true;
                }
                catch (Exception )
                {

                }
                
            }

            if (formats.Contains("FileGroupDescriptorW"))
            {
                var stream = dataObject.GetStreamByFileGroupDescriptorW();

                try
                {
                    var image = Image.FromStream(stream);
                    _storedImage = Utils.ImageToBitmapImage(image);
                }
                catch (Exception ex)
                {

                }

                if (formats.Contains("DragImageBits"))
                {
                    var bitmapSource = dataObject.GetBitmapSourceByDragImageBits();
                    ScanImage = bitmapSource;

                    return true;
                }
            }

            return false;
        }

        private bool CanMouseMove(MouseData mouseData)
        {
            return true;
        }

        private bool CanMouseDown(MouseData mouseData)
        {
            return true;
        }

        private void MouseMove(MouseData mouseData)
        {

        }

        private void MouseDown(MouseData mouseData)
        {

        }

        private bool CanOCRScanClick()
        {
            return true;
        }

        private bool CanScreenshotClick()
        {
            return true;
        }

        private bool CanUploadClick()
        {
            return true;
        }

        private void ScreenshotClick()
        {

        }

        private void ControlOpen()
        {
            UploadClick();
        }

        private void UploadClick()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.jpg;*.png;*.tiff;*.bmp";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog().Value)
            {
                var stream = openFileDialog.OpenFile();

                var image = Image.FromStream(stream);

                ScanImage = Utils.ImageToBitmapImage(image);
                _storedImage = Utils.ImageToBitmapImage(image);
                OCRScanButtonEnabled = true;
            }
        }

        private void ClipboardClick()
        {

        }

        private void ControlPaste()
        {

        }

        private async void OCRScanClick()
        {
            OCRScanButtonEnabled = false;
            var stream = Utils.BitmapSourceToStream(_storedImage);

            //var memoryStream = new MemoryStream();
            //var image = Image.FromStream(stream);
            //image.Save(memoryStream, ImageFormat.Jpeg);
            //await stream.CopyToAsync(memoryStream);
            
            var result = await _ocrSpaceService.GetTextAsync(stream);
            var parsedResult = result.ParsedResults.FirstOrDefault();

            if(parsedResult != null)
            {
                OCRText = parsedResult.ParsedText;
                TranslateButtonEnabled = true;
            }
            else
            {
                OCRText = "No results";
            }

            OCRScanButtonEnabled = true;
        }

        private void ImageDrop(DataObject dataObject)
        {
            ScanImage = _storedImage;
            OCRScanButtonEnabled = true;
        }

        private bool CanDrop(DataObject dataObject)
        {
            return true;   
        }

        
    }
}
