using Godot;

public partial class LabClient
{
    private void ApplyLabTheme()
    {
        Theme=new Theme{DefaultFont=font,DefaultFontSize=18};
        var text=new Color(0.86f,0.91f,0.91f);var muted=new Color(0.58f,0.68f,0.7f);
        Theme.SetColor("font_color","Label",text);
        Theme.SetColor("font_color","Button",text);
        Theme.SetColor("font_hover_color","Button",Colors.White);
        Theme.SetColor("font_disabled_color","Button",muted);
        Theme.SetStylebox("normal","Button",Card(new Color(0.085f,0.13f,0.15f),new Color(0.16f,0.24f,0.26f),7));
        Theme.SetStylebox("hover","Button",Card(new Color(0.10f,0.22f,0.24f),new Color(0.2f,0.52f,0.49f),7));
        Theme.SetStylebox("pressed","Button",Card(new Color(0.055f,0.28f,0.28f),new Color(0.22f,0.66f,0.6f),7));
        Theme.SetStylebox("focus","Button",Card(new Color(0,0,0,0),new Color(0.24f,0.72f,0.65f),7));
        Theme.SetStylebox("disabled","Button",Card(new Color(0.065f,0.09f,0.11f),new Color(0.1f,0.15f,0.17f),7));
        Theme.SetStylebox("panel","PanelContainer",Card(new Color(0.055f,0.085f,0.1f),new Color(0.14f,0.21f,0.23f),12,14));
        Theme.SetStylebox("normal","LineEdit",Card(new Color(0.025f,0.045f,0.06f),new Color(0.16f,0.25f,0.27f),6));
        Theme.SetColor("font_color","LineEdit",text);
        Theme.SetStylebox("panel","PopupMenu",Card(new Color(0.055f,0.095f,0.11f),new Color(0.16f,0.26f,0.29f),8));
    }
    private static StyleBoxFlat Card(Color color,Color border,int radius,int padding=8)=>new()
    {
        BgColor=color,BorderColor=border,BorderWidthLeft=1,BorderWidthTop=1,BorderWidthRight=1,BorderWidthBottom=1,
        CornerRadiusTopLeft=radius,CornerRadiusTopRight=radius,CornerRadiusBottomLeft=radius,CornerRadiusBottomRight=radius,
        ContentMarginLeft=padding,ContentMarginRight=padding,ContentMarginTop=padding,ContentMarginBottom=padding
    };
}
