"""Builds the README images from demo snapshots.

Usage: python tools/make-readme-images.py <snapshot-folder>
The folder holds PNGs made with `WindFreeControl.exe --demo --snapshot ...`:
control-dark, control-light, flyout-dark, flyout-light, settings-dark, control-en.
Needs Pillow.
"""
import os
import sys

from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT = os.path.join(os.path.dirname(__file__), "..", "docs", "images")
ICON = os.path.join(os.path.dirname(__file__), "..", "src", "WindFreeControl.App", "Assets", "app.ico")
FONT_BOLD = r"C:\Windows\Fonts\segoeuib.ttf"
FONT = r"C:\Windows\Fonts\segoeui.ttf"


def rounded(image, radius):
    mask = Image.new("L", image.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, image.width - 1, image.height - 1), radius=radius, fill=255)
    out = Image.new("RGBA", image.size, (0, 0, 0, 0))
    out.paste(image.convert("RGBA"), (0, 0), mask)
    return out


def with_shadow(image, blur=24, offset=10, opacity=110):
    pad = blur * 2
    canvas = Image.new("RGBA", (image.width + pad * 2, image.height + pad * 2), (0, 0, 0, 0))
    shadow = Image.new("RGBA", image.size, (0, 0, 0, opacity))
    shadow.putalpha(Image.eval(image.split()[3], lambda a: a * opacity // 255))
    canvas.paste(shadow, (pad, pad + offset), shadow)
    canvas = canvas.filter(ImageFilter.GaussianBlur(blur))
    canvas.paste(image, (pad, pad), image)
    return canvas


def crop_flyout(path):
    # The snapshot shows the panel on a plain window background; crop to whatever differs from it,
    # skipping the title bar row.
    image = Image.open(path).convert("RGB")
    background = image.getpixel((5, image.height - 5))
    diff = Image.new("L", image.size, 0)
    pixels, out = image.load(), diff.load()
    for y in range(48, image.height):
        for x in range(image.width):
            if sum(abs(a - b) for a, b in zip(pixels[x, y], background)) > 12:
                out[x, y] = 255
    return image.crop(diff.getbbox()).convert("RGBA")


def gradient(size, top, bottom):
    image = Image.new("RGBA", size)
    draw = ImageDraw.Draw(image)
    for y in range(size[1]):
        t = y / (size[1] - 1)
        draw.line([(0, y), (size[0], y)], fill=tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,))
    return image


def hero(src):
    canvas = gradient((1600, 860), (29, 78, 216), (15, 23, 42))
    draw = ImageDraw.Draw(canvas)
    icon = Image.open(ICON)
    icon.size = (256, 256)
    icon.load()
    icon = icon.convert("RGBA").resize((112, 112), Image.LANCZOS)
    canvas.paste(icon, (110, 250), icon)
    draw.text((110, 390), "AirTray", font=ImageFont.truetype(FONT_BOLD, 96), fill="white")
    sub = ImageFont.truetype(FONT, 34)
    draw.text((114, 510), "Samsung WindFree air conditioner,", font=sub, fill=(219, 234, 254))
    draw.text((114, 556), "right in the Windows tray.", font=sub, fill=(219, 234, 254))
    small = ImageFont.truetype(FONT, 24)
    draw.text((114, 640), "WinUI 3  ·  .NET 10 Native AOT  ·  SmartThings API", font=small, fill=(147, 197, 253))

    control = Image.open(os.path.join(src, "control-dark.png")).convert("RGBA")
    control = control.crop((0, 0, control.width, 760))
    control = with_shadow(rounded(control, 18))
    canvas.paste(control, (900, 30), control)

    flyout = with_shadow(rounded(crop_flyout(os.path.join(src, "flyout-light.png")), 14))
    canvas.paste(flyout, (1150, 470), flyout)
    return canvas.convert("RGB")


def pair(left, right, height=None, radius=16, gap=40, background=(246, 246, 246)):
    if height:
        left = left.crop((0, 0, left.width, min(height, left.height)))
        right = right.crop((0, 0, right.width, min(height, right.height)))
    left = with_shadow(rounded(left, radius), blur=16, offset=6, opacity=70)
    right = with_shadow(rounded(right, radius), blur=16, offset=6, opacity=70)
    canvas = Image.new("RGBA", (left.width + right.width + gap, max(left.height, right.height)), background + (255,))
    canvas.paste(left, (0, 0), left)
    canvas.paste(right, (left.width + gap, 0), right)
    return canvas.convert("RGB")


def main(src):
    os.makedirs(OUT, exist_ok=True)
    hero(src).save(os.path.join(OUT, "hero.png"), optimize=True)
    load = lambda name: Image.open(os.path.join(src, name)).convert("RGBA")
    pair(load("control-dark.png"), load("control-light.png"), height=1100).save(os.path.join(OUT, "control.png"), optimize=True)
    pair(crop_flyout(os.path.join(src, "flyout-dark.png")), crop_flyout(os.path.join(src, "flyout-light.png"))).save(os.path.join(OUT, "flyout.png"), optimize=True)
    settings = load("settings-dark.png")
    settings = settings.crop((0, 0, settings.width, 560))
    pair(settings, load("control-en.png"), height=560).save(os.path.join(OUT, "settings-language.png"), optimize=True)
    print("images written to", os.path.abspath(OUT))


if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    main(sys.argv[1])
