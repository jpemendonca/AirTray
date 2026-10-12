"""Draws the app and tray icons. Needs Pillow: python -m pip install pillow."""
import math
import os
import sys

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(__file__), "..", "src", "WindFreeControl.App", "Assets")
SIZES = [16, 20, 24, 32, 40, 48, 64, 256]


def waves(draw, size, color, width):
    # Three soft horizontal waves: the "wind" mark used across the app.
    for row in range(3):
        y0 = size * (0.34 + row * 0.16)
        points = []
        for i in range(41):
            t = i / 40
            x = size * (0.2 + 0.6 * t)
            y = y0 + math.sin(t * math.pi * 2) * size * 0.045
            points.append((x, y))
        draw.line(points, fill=color, width=width, joint="curve")
        r = width / 2
        for x, y in (points[0], points[-1]):
            draw.ellipse((x - r, y - r, x + r, y + r), fill=color)


def render(size, top, bottom, mark):
    scale = 4
    big = size * scale
    image = Image.new("RGBA", (big, big), (0, 0, 0, 0))
    gradient = Image.new("RGBA", (big, big))
    gd = ImageDraw.Draw(gradient)
    for y in range(big):
        t = y / (big - 1)
        gd.line([(0, y), (big, y)], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,))
    mask = Image.new("L", (big, big), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, big - 1, big - 1), radius=big * 0.22, fill=255)
    image.paste(gradient, (0, 0), mask)
    waves(ImageDraw.Draw(image), big, mark, max(scale, int(big * 0.075)))
    return image.resize((size, size), Image.LANCZOS)


def save_ico(name, top, bottom, mark=(255, 255, 255, 255)):
    images = [render(s, top, bottom, mark) for s in SIZES]
    images[-1].save(os.path.join(OUT, name), sizes=[(s, s) for s in SIZES], append_images=images[:-1])
    return images


def main():
    os.makedirs(OUT, exist_ok=True)
    blue = ((96, 165, 250), (29, 78, 216))
    grey = ((161, 161, 170), (82, 82, 91))
    app = save_ico("app.ico", *blue)
    app[0].save(os.path.join(OUT, "app-16.png"))
    render(32, *blue, (255, 255, 255, 255)).save(os.path.join(OUT, "app-32.png"))
    save_ico("tray-on.ico", *blue)
    save_ico("tray-off.ico", *grey)
    print("icons written to", os.path.abspath(OUT))


if __name__ == "__main__":
    sys.exit(main())
