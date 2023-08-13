using Godot;
using System;
using System.Collections;

class PixelMask
{
    private Image _texture;
    private Vector2 _size;
    private Vector2 _position;
    private BitArray _mask;

    public PixelMask(Texture texture, Vector2 position)
    {
        // texture.DynamicObjects
        this._texture = texture.GetData();
        this._size = texture.GetSize();
        this._position = position - this._size / 2;
        this._mask = this._bitifY(texture);
    }

    private BitArray _bitifY(Texture texture)
    {
        var size = texture.GetSize();
        var mask = new BitArray((int)(size.x * size.y));

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var pixel = _texture.GetPixel(x, y);
                if (pixel.a == 0)
                {
                    continue;
                }
                mask.Set((int)(x + y * size.x), true);
            }
        }
        return mask;
    }

    public Vector2? CheckCollision(PixelMask other)
    {
        // GD.Print("this._position", this._position, "other._position", other._position);
        // GD.Print("this._size", this._size, "other._size", other._size);

        var area = new Rect2(this._position, this._size);
        var otherArea = new Rect2(other._position, other._size);
        if (!area.Intersects(otherArea))
        {
            return null;
        }

        return Vector2.Zero;

        var thisTop = this._position.y - this._size.y;
        var thisBottom = this._position.y;
        var thisLeft = this._position.x;
        var thisRight = this._position.x + this._size.x;

        var otherTop = other._position.y - other._size.y;
        var otherBottom = other._position.y;
        var otherLeft = other._position.x;
        var otherRight = other._position.x + other._size.x;

        var top = Math.Max(thisTop, otherTop);
        var bottom = Math.Min(thisBottom, otherBottom);
        var left = Math.Max(thisLeft, otherLeft);
        var right = Math.Min(thisRight, otherRight);

        for (var y = top; y < bottom; y++)
        {
            for (var x = left; x < right; x++)
            {
                var thisPoint = new Vector2(x - thisLeft, y - thisTop);
                var otherPoint = new Vector2(x - otherLeft, y - otherTop);
                if (this.Contains(thisPoint) && other.Contains(otherPoint))
                {
                    return thisPoint;
                }
            }
        }

        return null;
    }

    public bool Contains(Vector2 point)
    {
        // if (_mask[localY * (int)_size.x + localX] && other._mask[otherLocalY * (int)other._size.x + otherLocalX])
        // {
        //     // return new Vector2(otherLocalX - localX, otherLocalY - localY); // Collision detected
        //     return new Vector2(x, y); // Collision detected
        // }
        return this._mask[(int)point.y * (int)this._size.x + (int)point.x];
    }

    public Vector2? CollidesWith(PixelMask other)
    {
        if (!new Rect2(_position, _size).Intersects(new Rect2(other._position, other._size)))
        {
            return null;
        }

        // Check for detailed collision using bitmasks
        int startX = (int)Math.Max(_position.x, other._position.x);
        int startY = (int)Math.Max(_position.y, other._position.y);
        int endX = (int)Math.Min(_position.x + _size.x, other._position.x + other._size.x);
        int endY = (int)Math.Min(_position.y + _size.y, other._position.y + other._size.y);

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                int localX = x - (int)_position.x;
                int localY = y - (int)_position.y;
                int otherLocalX = x - (int)other._position.x;
                int otherLocalY = y - (int)other._position.y;
                // if (_mask[localY * (int)_size.x + localX] && other._mask[otherLocalY * (int)other._size.x + otherLocalX])
                if (
                    Contains(new Vector2(localX, localY))
                    && other.Contains(new Vector2(otherLocalX, otherLocalY))
                )
                {
                    // return new Vector2(otherLocalX - localX, otherLocalY - localY); // Collision detected
                    return new Vector2(x, y); // Collision detected
                }
            }
        }

        return null; // No collision detected
    }

    public Vector2? Collides(PixelMask other)
    {
        if (!new Rect2(_position, _size).Intersects(new Rect2(other._position, other._size)))
        {
            return null;
        }

        var left = Mathf.FloorToInt(Math.Max(_position.x, other._position.x));
        var top = Mathf.FloorToInt(Math.Max(_position.y, other._position.y));
        var right = Mathf.FloorToInt(
            Math.Min(_position.x + _size.x, other._position.x + other._size.x)
        );
        var bottom = Mathf.FloorToInt(
            Math.Min(_position.y + _size.y, other._position.y + other._size.y)
        );

        var globalX = (left + right) / 2;
        var globalY = (top + bottom) / 2;
        for (var y = top; y < bottom; y++)
        {
            for (var x = left; x < right; x++)
            {
                // var pos = new Vector2(x, y);
                // var local = pos - _position;
                // var otherLocal = pos - other._position;
                var localX = x - Mathf.FloorToInt(_position.x);
                var localY = y - Mathf.FloorToInt(_position.y);
                var otherLocalX = x - Mathf.FloorToInt(other._position.x);
                var otherLocalY = y - Mathf.FloorToInt(other._position.y);

                if (
                    Contains(new Vector2(localX, localY))
                    && other.Contains(new Vector2(otherLocalX, otherLocalY))
                )
                {
                    // return new Vector2(otherLocalX - localX, otherLocalY - localY); // Collision detected
                    return new Vector2(x, y); // Collision detected
                }
            }
        }

        return null;
    }
}
