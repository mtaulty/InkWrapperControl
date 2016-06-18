namespace App26
{
  using System;
  using System.Threading.Tasks;
  using Utility;
  using Windows.Devices.Input;
  using Windows.UI.Input.Inking;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  using Windows.UI.Xaml.Input;
  using Windows.UI.Xaml.Markup;

  [ContentProperty(Name = "WrappedContent")] // TODO: not sure this is working like I expect.

  public sealed partial class InkWrapperControl : UserControl
  {
    static Lazy<InkCanvas> inkCanvas;
    static InkWrapperControl currentControl;

    public static readonly DependencyProperty WrappedContentProperty =
      DependencyProperty.Register(
        "WrappedContent",
        typeof(object),
        typeof(InkWrapperControl),
        new PropertyMetadata(null, OnWrappedContentChanged));

    public static readonly DependencyProperty InkStrokesProperty =
      DependencyProperty.Register(
        "InkStrokes",
        typeof(InkStrokeContainer),
        typeof(InkWrapperControl),
        new PropertyMetadata(null, OnInkStrokesChangedAsync));

    static InkWrapperControl()
    {
      inkCanvas = new Lazy<InkCanvas>(
        () =>
        {
          var canvas = new InkCanvas();
          return (canvas);
        }
      );
    }
    public InkWrapperControl()
    {
      this.InitializeComponent();
      this.Loaded += OnLoaded;
    }

    async void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Bit of a hack. When this control first comes up it needs to render the ink contained
      // in its InkStokes container into the image to ensure that's on screen. Or...it would
      // need to persist the image itself.
      // This could be done when the InkStrokes property changes BUT it's likely that this
      // would change before the control has sized itself and then how do we size the image?
      // Sizing the image (and/or ink) is a challenge anyway tbh.
      this.inkImage.Source = await InkDrawing.InkToBitmapSourceAtSizeAsync(
        (int)this.gridForeground.ActualWidth,
        (int)this.gridForeground.ActualHeight,
        this.InkStrokes);
    }
    public InkStrokeContainer InkStrokes
    {
      get
      {
        return ((InkStrokeContainer)base.GetValue(InkStrokesProperty));
      }
      set
      {
        base.SetValue(InkStrokesProperty, value);
      }
    }
    public object WrappedContent
    {
      get
      {
        return (base.GetValue(WrappedContentProperty));
      }
      set
      {
        base.SetValue(WrappedContentProperty, value);
      }
    }
    static void OnWrappedContentChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      var control = sender as InkWrapperControl;

      if (args.OldValue != null)
      {
        control?.gridForeground.Children.Remove((UIElement)args.OldValue);
      }
      if (args.NewValue != null)
      {
        control?.gridForeground.Children.Add((UIElement)args.NewValue);
      }
    }
    static async void OnInkStrokesChangedAsync(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      var control = (InkWrapperControl)sender;
      
      if ((control.gridForeground.ActualWidth > 0.0f) &&
        (control.gridForeground.ActualHeight > 0.0f))
      {
        // If the ink stroles collection has changed we need to redraw the image that
        // represents it. See comment in the constructor for a similar/related
        // challenge.
        control.inkImage.Source = await InkDrawing.InkToBitmapSourceAtSizeAsync(
          (int)control.gridForeground.ActualWidth,
          (int)control.gridForeground.ActualHeight,
          control.InkStrokes);
      }                
    }
    async void OnControlPointerEntered(object sender, PointerRoutedEventArgs e)
    {
      if (e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
      {
        if (this != currentControl)
        {
          // TBD - problem here of calling an async method from a handler that we can't
          // really do anything with other than fire/forget.
          await TearDownInkingUIOnCurrentControlAsync();
          this.SetupInkingUIOnThisControl();
          currentControl = this;
        }
      }
    }
    static async Task TearDownInkingUIOnCurrentControlAsync()
    {
      // If we have a current control, we've moved away from it so take away any
      // of our 'infrastructure'
      if (currentControl != null)
      {
        currentControl.inkImage.Source = await InkDrawing.InkToBitmapSourceAtSizeAsync(
          (int)currentControl.gridForeground.ActualWidth,
          (int)currentControl.gridForeground.ActualHeight,
          currentControl.InkStrokes);

        currentControl.gridForeground.Children.Remove(inkCanvas.Value);

        currentControl.inkImage.Visibility = Visibility.Visible;
      }
    }
    void SetupInkingUIOnThisControl()
    {
      inkCanvas.Value.InkPresenter.StrokeContainer = this.InkStrokes;
      this.inkImage.Visibility = Visibility.Collapsed;
      this.gridForeground.Children.Add(inkCanvas.Value);
    }

    void OnControlPointerExited(object sender, PointerRoutedEventArgs e)
    {
      // NB: We do no work here just yet because I think our messing with the visual
      // tree above will generate multiple enter/exit events on the pen and if we
      // take action here we get in a mess.
    }
  }
}
