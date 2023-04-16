using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Networking;

public class PaintCanvas : MonoBehaviour
{
    private Texture2D _texture = null;
    private RenderTexture _textureRender = null;
    private BoxCollider2D _collider = null;
    private float _timerDrawCall = 0f;
    private float _pixelsPerUnit = 100f;
    private float _pixelsHalfTextureX = 0f;
    private float _pixelsHalfTextureY = 0f;

    [Header("Texture Size")]
    public int textureSizeWidth = 1200;

    public int textureSizeHeight = 700;
    [Header("Texture Filter Mode")]
    public FilterMode filterMode = FilterMode.Bilinear;
    [Header("Texture Wrap Mode")]
    public TextureWrapMode textureWrapMode = TextureWrapMode.Clamp;
    [Header("Camera as object")]
    public Camera camera = null;

    [Header("Default Texture color")] public Color defaultColor = new Color(1f, 1f, 1f, 0.75f);
    [Header("Amount of draw call per second")]
    public int drawCallPerSec = 60;

    public Texture2D example;

    [Header("Brush type")] public Brush brush = Brush.Circle;
    [Header("Brush diameter")] public int brushDiameter = 15;
    [Header("Brush color")] public Color brushColor = Color.black;

    // Start is called before the first frame update
    private void Start()
    {
        _timerDrawCall = 0f;
        
        if (_pixelsPerUnit == 0)
            throw new Exception("_pixelsPerUnit cannot be 0!");
        
        // Initialize Texture2D
        _texture = new Texture2D(textureSizeWidth, textureSizeHeight, TextureFormat.RGBA32, false)
        {
            name = "paint",
            filterMode = filterMode,
            wrapMode = textureWrapMode
        };

        // Byte[] textureBytes = _texture.GetRawTextureData();
        //
        // Debug.Log(textureBytes[0]);
        // Debug.Log(textureBytes[1]);
        // Debug.Log(textureBytes[2]);
        // Debug.Log(textureBytes[3]);

        ClearCanvas();

        // Initialize 
        // _textureRender = new RenderTexture(textureSize, textureSize, 24, RenderTextureFormat.ARGB32)
        // {
        //     name = "paint_render",
        //     filterMode = filterMode,
        //     wrapMode = textureWrapMode
        // };

        // Initialize Sprite
        var sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, textureSizeWidth, textureSizeHeight),
            new Vector2(0.5f, 0.5f), _pixelsPerUnit);

        sprite.name = "paint";
        
        // First draw renderer texture
        // Graphics.Blit(_texture, _textureRender);

        // Set Sprite to SpriteRenderer
        var renderer = GetComponent<SpriteRenderer>();
        // renderer.material 
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Simple;

        // Calculate halfsize of Texture2D
        _collider = GetComponent<BoxCollider2D>();
        _collider.size = new Vector2(textureSizeWidth / _pixelsPerUnit, textureSizeHeight / _pixelsPerUnit);

        _pixelsHalfTextureX = textureSizeWidth / 2 / _pixelsPerUnit;
        _pixelsHalfTextureY = textureSizeHeight / 2 / _pixelsPerUnit;

        // brushColor = UnityEngine.Random.ColorHSV();
        // Debug.Log(_texture.);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameController.Instance.drawUI.activeSelf)
            return;
        
        _timerDrawCall += Time.deltaTime;

        if (_timerDrawCall < 1f / drawCallPerSec)
            return;

        _timerDrawCall = 0f;

        if (Input.GetMouseButton(0))
        {
            var mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
            var direction = (Vector2)mousePosition;
            var hitInfo = Physics2D.Raycast(mousePosition, direction);
            
            if (hitInfo.collider != null)
            {
                var rayX = (int)((hitInfo.point.x + -transform.position.x + _pixelsHalfTextureX) * _pixelsPerUnit);
                var rayY = (int)((hitInfo.point.y + -transform.position.y + _pixelsHalfTextureY) * _pixelsPerUnit);

                ChangePixels(rayX, rayY, brushDiameter, brushColor, brush);

                // _texture.SetPixel(rayX, rayY, Color.black);
                // Graphics.Blit(_texture, _textureRender);
                _texture.Apply();

                GameController.Instance.networkController.SendPaintEvent(
                    new WebSocketController.PaintData()
                    {
                        x = rayX,
                        y = rayY,
                        diameter = brushDiameter,
                        color = brushColor,
                        brushType = brush,
                    }
                );
            }
        }
    }

    public void ClearCanvas()
    {
        if (_texture == null)
            return;
        
        // Set white color
        Color[] pixels = Enumerable.Repeat(defaultColor, textureSizeWidth * textureSizeHeight).ToArray();
        _texture.SetPixels(pixels);
        _texture.Apply();
    }

    public void OtherDraw(WebSocketController.PaintData[] data)
    {
        if (data.Length < 1)
            return;

        if (_texture == null)
            return;
        
        foreach (var paintData in data)
        {
            ChangePixels(paintData.x, paintData.y, paintData.diameter, paintData.color, paintData.brushType);
        }
        
        _texture.Apply();
    }

    private void ChangePixels(int centerX, int centerY, int diameter, Color color, Brush typeBrush)
    {
        int radius = diameter / 2;
        int a = 0;
        int b = 0;
        
        for (int y = centerY - radius; y < centerY + radius; y++)
        {
            for (int x = centerX - radius; x < centerX + radius; x++)
            {
                if (x < 0 || y < 0 || x >= textureSizeWidth || y >= textureSizeHeight) continue;
    
                a = x - centerX;
                b = y - centerY;

                if (typeBrush == Brush.Circle)
                {
                    if ((a * a) + (b * b) <= (radius * radius))
                        _texture.SetPixel(x, y, color);
                }
                else
                {
                    _texture.SetPixel(x, y, color);
                }
            }
        }
    }

    public enum Brush {
        Square,
        Circle,
    }
}
