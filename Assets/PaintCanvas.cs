using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PaintCanvas : MonoBehaviour
{
    private Texture2D _texture = null;
    private BoxCollider2D _collider = null;
    private float _timerDrawCall = 0f;
    private float _pixelsPerUnit = 100f;
    private float _pixelsHalfTextureX = 0f;
    private float _pixelsHalfTextureY = 0f;

    public int textureSize = 256;
    public FilterMode filterMode = FilterMode.Point;
    public TextureWrapMode textureWrapMode = TextureWrapMode.Clamp;
    public Camera camera = null;
    public int drawCallPerSec = 60;

    // Start is called before the first frame update
    private void Start()
    {
        _timerDrawCall = 0f;
        
        if (_pixelsPerUnit == 0)
            throw new Exception("_pixelsPerUnit cannot be 0!");
        
        // Initialize Texture2D
        _texture = new Texture2D(textureSize, textureSize)
        {
            name = "paint",
            filterMode = filterMode,
            wrapMode = textureWrapMode
        };

        // Initialize Sprite
        var sprite = Sprite.Create(_texture, new Rect(0.0f, 0.0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f), _pixelsPerUnit);

        sprite.name = "paint";

        // Set Sprite to SpriteRenderer
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Simple;

        // Calculate halfsize of Texture2D
        _collider = GetComponent<BoxCollider2D>();
        _collider.size = new Vector2(textureSize / _pixelsPerUnit, textureSize / _pixelsPerUnit);

        _pixelsHalfTextureX = textureSize / 2 / _pixelsPerUnit;
        _pixelsHalfTextureY = textureSize / 2 / _pixelsPerUnit;
    }

    // Update is called once per frame
    private void Update()
    {
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
                // Draw debug Draw line
                Debug.DrawLine(mousePosition, (Vector2)transform.position, Color.yellow);

                var rayX = (int)((hitInfo.point.x + -transform.position.x + _pixelsHalfTextureX) * _pixelsPerUnit);
                var rayY = (int)((hitInfo.point.y + -transform.position.y + _pixelsHalfTextureY) * _pixelsPerUnit);


                _texture.SetPixel(rayX, rayY, Color.black);
                _texture.Apply();
            }
        }
    }

    private void ChangePixels(int centerX, int centerY, int diameter, Color color)
    {
        int radius = diameter / 2;
        int a = 0;
        int b = 0;
        
        for (int y = centerY - radius; y < centerY + radius; y++)
        {
            for (int x = centerX - radius; x < centerX + radius; x++)
            {
                if (x < 0 || y < 0 || x >= textureSize || y >= textureSize) continue;

                a = x - centerX;
                b = y - centerY;

                if ((a * a) + (b * b) <= (radius * radius))
                    maskColors[x + y * maskedTexture.width] = color;
            }
        }
    }
}
