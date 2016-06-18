namespace App26
{
  using System.ComponentModel;
  using Windows.UI.Input.Inking;

  class ViewModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public int Number { get; set; }

    public bool HasStrokes
    {
      get
      {
        bool strokesPresent = this.strokeContainer != null;

        if (strokesPresent)
        {
          var strokes = this.strokeContainer.GetStrokes();
          strokesPresent = strokes.Count > 0;
        }

        return (strokesPresent);
      }
    }

    public InkStrokeContainer Strokes
    {
      get
      {
        if (this.strokeContainer == null)
        {
          this.strokeContainer = new InkStrokeContainer();
        }
        return (this.strokeContainer);
      }
      private set
      {
        this.strokeContainer = value;
      }
    }
    public void FireStrokesChanged()
    {
      // Been extremely lazy with not doing this properly.
      var temp = this.Strokes;
      this.Strokes = null;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Strokes"));
      this.Strokes = temp;
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Strokes"));
    }
    InkStrokeContainer strokeContainer;
  }
}
