namespace App26
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.IO;
  using System.Linq;
  using Windows.Storage;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;

  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();

      this.Loaded += (s, e) =>
      {
        this.dataContext =
          Enumerable
          .Range(1, 100)
          .Select(
            i => new ViewModel()
            {
              Number = i
            }
          ).ToList();

        this.DataContext = this.dataContext;
      };
    }
    async void OnSaveAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      // Clearly, this is not an efficient way to save anything.
      foreach (var item in this.dataContext)
      {
        string filename = $"{item.Number}.isf";

        if (item.HasStrokes)
        {
          var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            filename, CreationCollisionOption.ReplaceExisting);

          using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
          {
            await item.Strokes.SaveAsync(stream);
          }
        }
        else
        {
          try
          {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(
              filename);

            await file.DeleteAsync();
          }
          catch (FileNotFoundException)
          {
          }
        }
      }
    }
    async void OnLoadAsync(object sender, RoutedEventArgs e)
    {
      var folder = ApplicationData.Current.LocalFolder;

      var files = await folder.GetFilesAsync();

      foreach (var file in files)
      {
        var number = file.Name.Split('.')[0];
        var intNumber = int.Parse(number);
        var viewModel = this.dataContext[intNumber - 1];

        using (var stream = await file.OpenReadAsync())
        {
          await viewModel.Strokes.LoadAsync(stream);
        }
        viewModel.FireStrokesChanged();
      }
    }
    List<ViewModel> dataContext;
  }
}
