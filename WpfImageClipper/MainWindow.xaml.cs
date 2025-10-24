using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfImageClipper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private OpenFileDialog? _openImageDialog;
    private SaveFileDialog? _saveImageDialog;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        _openImageDialog ??= new OpenFileDialog()
        {
            Filter = "Any Image|*.jpg;*.jpeg;*.png;*.bmp",
            CheckFileExists = true,
        };

        if (_openImageDialog.ShowDialog() != true)
        {
            return;
        }

        using var file = File.OpenRead(_openImageDialog.FileName);

        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = ms;
        bitmap.EndInit();

        _imageClipper.Source = bitmap;
    }

    private void ClearPathButton_Click(object sender, RoutedEventArgs e)
    {
        _imageClipper.AreaPoints.Clear();
        _imageClipper.IsAreaClosed = false;
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        _saveImageDialog ??= new SaveFileDialog()
        {
            Filter = "JPEG Image|*.jpg;*.jpeg|PNG Image|*.png|BMP Image|*.bmp",
            CheckPathExists = true,
        };

        if (_saveImageDialog.ShowDialog() != true)
        {
            return;
        }

        using var file = File.Create(_saveImageDialog.FileName);
        BitmapEncoder encoder = System.IO.Path.GetExtension(_saveImageDialog.FileName).ToLower() switch
        {
            ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
            ".png" => new PngBitmapEncoder(),
            ".bmp" => new BmpBitmapEncoder(),
            _ => new BmpBitmapEncoder(),
        };

        encoder.Frames.Add(BitmapFrame.Create(_imageClipper.GetClippedImage()));
        encoder.Save(file);
    }
}