"""
Generate all test images for the multimodal vision benchmark.

Creates programmatic test images with known ground truth answers across
6 categories: OCR, Document Understanding, Chart Reading, Visual QA,
Code from Visual, and Math.
"""

import os

from PIL import Image, ImageDraw, ImageFont

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt


ASSET_DIR = os.path.join(os.path.dirname(__file__), "multimodal_assets")


def _get_font(size: int = 20, bold: bool = False, mono: bool = False) -> ImageFont.FreeTypeFont:
    """Try to load a decent font, fall back to default."""
    candidates = []
    if mono:
        candidates = ["consola.ttf", "cour.ttf", "Courier New.ttf", "DejaVuSansMono.ttf"]
    elif bold:
        candidates = ["arialbd.ttf", "calibrib.ttf", "DejaVuSans-Bold.ttf"]
    else:
        candidates = ["arial.ttf", "calibri.ttf", "DejaVuSans.ttf"]
    for name in candidates:
        try:
            return ImageFont.truetype(name, size)
        except (OSError, IOError):
            pass
    return ImageFont.load_default(size=size)


def _get_italic_font(size: int = 20) -> ImageFont.FreeTypeFont:
    """Try to load an italic font."""
    candidates = ["ariali.ttf", "calibrii.ttf", "DejaVuSans-Oblique.ttf"]
    for name in candidates:
        try:
            return ImageFont.truetype(name, size)
        except (OSError, IOError):
            pass
    return _get_font(size)


# ---------------------------------------------------------------------------
# Category 1: OCR
# ---------------------------------------------------------------------------

OCR_CODE_TEXT = """\
public static int Add(int a, int b)
{
    return a + b;
}"""


def gen_ocr_code() -> None:
    img = Image.new("RGB", (800, 300), "white")
    draw = ImageDraw.Draw(img)
    font = _get_font(22, mono=True)
    draw.text((30, 30), OCR_CODE_TEXT, fill="black", font=font)
    img.save(os.path.join(ASSET_DIR, "ocr_code.png"))


OCR_MIXED_FONTS_HEADER = "Quarterly Report"
OCR_MIXED_FONTS_BODY = "Revenue increased by 15% compared to the previous quarter.\nNet profit margins remained stable at 22%."
OCR_MIXED_FONTS_FULL = OCR_MIXED_FONTS_HEADER + "\n" + OCR_MIXED_FONTS_BODY


def gen_ocr_mixed_fonts() -> None:
    img = Image.new("RGB", (800, 300), "white")
    draw = ImageDraw.Draw(img)
    bold_font = _get_font(30, bold=True)
    body_font = _get_font(20)
    draw.text((30, 30), OCR_MIXED_FONTS_HEADER, fill="black", font=bold_font)
    draw.text((30, 80), OCR_MIXED_FONTS_BODY, fill="black", font=body_font)
    img.save(os.path.join(ASSET_DIR, "ocr_mixed_fonts.png"))


OCR_NUMBERS_GRID = [
    [12, 45, 78],
    [23, 56, 89],
    [34, 67, 91],
    [48, 72, 35],
]
OCR_NUMBERS_FLAT = "12 45 78 23 56 89 34 67 91 48 72 35"


def gen_ocr_numbers() -> None:
    img = Image.new("RGB", (500, 400), "white")
    draw = ImageDraw.Draw(img)
    font = _get_font(24, mono=True)
    y = 40
    col_width = 120
    # Draw header line
    draw.line([(30, y - 5), (30 + 3 * col_width, y - 5)], fill="gray", width=1)
    for row in OCR_NUMBERS_GRID:
        for ci, val in enumerate(row):
            draw.text((40 + ci * col_width, y), str(val), fill="black", font=font)
        y += 50
        draw.line([(30, y - 5), (30 + 3 * col_width, y - 5)], fill="gray", width=1)
    # Vertical lines
    for ci in range(4):
        x = 30 + ci * col_width
        draw.line([(x, 35), (x, y - 5)], fill="gray", width=1)
    img.save(os.path.join(ASSET_DIR, "ocr_numbers.png"))


OCR_HANDWRITING_TEXT = "The quick brown fox jumps over the lazy dog"


def gen_ocr_handwriting_style() -> None:
    img = Image.new("RGB", (800, 200), "#fffff0")
    draw = ImageDraw.Draw(img)
    font = _get_italic_font(28)
    draw.text((30, 60), OCR_HANDWRITING_TEXT, fill="#333333", font=font)
    img.save(os.path.join(ASSET_DIR, "ocr_handwriting_style.png"))


# ---------------------------------------------------------------------------
# Category 2: Document Understanding
# ---------------------------------------------------------------------------

INVOICE_DATA = {
    "company": "Acme Corp",
    "date": "2025-03-15",
    "items": [
        {"description": "Widget A", "qty": 10, "price": 5.00},
        {"description": "Widget B", "qty": 5, "price": 12.50},
        {"description": "Service Fee", "qty": 1, "price": 25.00},
    ],
    "total": 137.50,
}


def gen_doc_invoice() -> None:
    img = Image.new("RGB", (800, 600), "white")
    draw = ImageDraw.Draw(img)
    title_font = _get_font(28, bold=True)
    header_font = _get_font(18, bold=True)
    font = _get_font(18)

    draw.text((30, 20), "INVOICE", fill="black", font=title_font)
    draw.text((30, 60), f"Company: {INVOICE_DATA['company']}", fill="black", font=font)
    draw.text((30, 90), f"Date: {INVOICE_DATA['date']}", fill="black", font=font)

    draw.line([(30, 130), (770, 130)], fill="black", width=2)
    draw.text((30, 140), "Description", fill="black", font=header_font)
    draw.text((400, 140), "Qty", fill="black", font=header_font)
    draw.text((550, 140), "Price", fill="black", font=header_font)
    draw.line([(30, 165), (770, 165)], fill="black", width=1)

    y = 175
    for item in INVOICE_DATA["items"]:
        draw.text((30, y), item["description"], fill="black", font=font)
        draw.text((400, y), str(item["qty"]), fill="black", font=font)
        draw.text((550, y), f"${item['price']:.2f}", fill="black", font=font)
        y += 35

    draw.line([(30, y + 5), (770, y + 5)], fill="black", width=2)
    draw.text((400, y + 15), "Total:", fill="black", font=header_font)
    draw.text((550, y + 15), f"${INVOICE_DATA['total']:.2f}", fill="black", font=header_font)
    img.save(os.path.join(ASSET_DIR, "doc_invoice.png"))


TABLE_HEADERS = ["Name", "Age", "City"]
TABLE_ROWS = [
    ["Alice", "30", "New York"],
    ["Bob", "25", "London"],
    ["Carol", "35", "Tokyo"],
    ["Dave", "28", "Paris"],
    ["Eve", "32", "Berlin"],
]


def gen_doc_table() -> None:
    img = Image.new("RGB", (600, 400), "white")
    draw = ImageDraw.Draw(img)
    header_font = _get_font(20, bold=True)
    font = _get_font(18)

    col_x = [30, 200, 320]
    y = 30
    for ci, hdr in enumerate(TABLE_HEADERS):
        draw.text((col_x[ci], y), hdr, fill="black", font=header_font)
    y += 30
    draw.line([(20, y), (560, y)], fill="black", width=2)
    y += 10
    for row in TABLE_ROWS:
        for ci, val in enumerate(row):
            draw.text((col_x[ci], y), val, fill="black", font=font)
        y += 30
    img.save(os.path.join(ASSET_DIR, "doc_table.png"))


FORM_FIELDS = {"Name": "Alice", "Email": "alice@test.com", "Role": "Developer"}


def gen_doc_form() -> None:
    img = Image.new("RGB", (600, 350), "#f5f5f5")
    draw = ImageDraw.Draw(img)
    title_font = _get_font(24, bold=True)
    label_font = _get_font(18, bold=True)
    value_font = _get_font(18)

    draw.text((30, 20), "Registration Form", fill="black", font=title_font)
    y = 70
    for label, value in FORM_FIELDS.items():
        draw.text((30, y), f"{label}:", fill="black", font=label_font)
        # Draw input box
        draw.rectangle([(150, y - 2), (500, y + 28)], outline="gray", width=1)
        draw.text((160, y + 2), value, fill="black", font=value_font)
        y += 55
    img.save(os.path.join(ASSET_DIR, "doc_form.png"))


# ---------------------------------------------------------------------------
# Category 3: Chart Reading
# ---------------------------------------------------------------------------

BAR_LABELS = ["Q1", "Q2", "Q3", "Q4"]
BAR_VALUES = [25, 40, 35, 50]


def gen_chart_bar() -> None:
    fig, ax = plt.subplots(figsize=(8, 6))
    bars = ax.bar(BAR_LABELS, BAR_VALUES, color=["#4285f4", "#ea4335", "#fbbc05", "#34a853"])
    ax.set_title("Quarterly Revenue ($M)", fontsize=16)
    ax.set_ylabel("Revenue")
    for bar, val in zip(bars, BAR_VALUES):
        ax.text(bar.get_x() + bar.get_width() / 2, bar.get_height() + 1,
                str(val), ha="center", va="bottom", fontsize=14)
    plt.tight_layout()
    fig.savefig(os.path.join(ASSET_DIR, "chart_bar.png"), dpi=100)
    plt.close(fig)


PIE_LABELS = ["Desktop", "Mobile", "Tablet"]
PIE_VALUES = [55, 30, 15]


def gen_chart_pie() -> None:
    fig, ax = plt.subplots(figsize=(8, 6))
    ax.pie(PIE_VALUES, labels=PIE_LABELS, autopct="%1.0f%%",
           colors=["#4285f4", "#ea4335", "#fbbc05"], startangle=90,
           textprops={"fontsize": 14})
    ax.set_title("Device Usage Distribution", fontsize=16)
    plt.tight_layout()
    fig.savefig(os.path.join(ASSET_DIR, "chart_pie.png"), dpi=100)
    plt.close(fig)


LINE_X = [1, 2, 3, 4, 5]
LINE_Y = [10, 25, 20, 35, 30]


def gen_chart_line() -> None:
    fig, ax = plt.subplots(figsize=(8, 6))
    ax.plot(LINE_X, LINE_Y, "o-", color="#4285f4", linewidth=2, markersize=8)
    for x, y in zip(LINE_X, LINE_Y):
        ax.annotate(str(y), (x, y), textcoords="offset points",
                    xytext=(0, 10), ha="center", fontsize=13)
    ax.set_title("Monthly Sales (units)", fontsize=16)
    ax.set_xlabel("Month")
    ax.set_ylabel("Sales")
    ax.set_xticks(LINE_X)
    plt.tight_layout()
    fig.savefig(os.path.join(ASSET_DIR, "chart_line.png"), dpi=100)
    plt.close(fig)


# ---------------------------------------------------------------------------
# Category 4: Visual QA
# ---------------------------------------------------------------------------

def gen_vqa_shapes() -> None:
    img = Image.new("RGB", (800, 400), "white")
    draw = ImageDraw.Draw(img)
    # Red circle
    draw.ellipse([(80, 100), (250, 270)], fill="red", outline="darkred", width=2)
    # Blue square
    draw.rectangle([(320, 100), (490, 270)], fill="blue", outline="darkblue", width=2)
    # Green triangle
    draw.polygon([(650, 100), (560, 270), (740, 270)], fill="green", outline="darkgreen")
    # Labels
    font = _get_font(16)
    draw.text((135, 285), "Circle", fill="black", font=font)
    draw.text((370, 285), "Square", fill="black", font=font)
    draw.text((615, 285), "Triangle", fill="black", font=font)
    img.save(os.path.join(ASSET_DIR, "vqa_shapes.png"))


def gen_vqa_layout() -> None:
    img = Image.new("RGB", (800, 600), "#f0f0f0")
    draw = ImageDraw.Draw(img)
    font = _get_font(18)
    # Header
    draw.rectangle([(0, 0), (800, 60)], fill="#333333")
    draw.text((20, 18), "Header / Navigation Bar", fill="white", font=font)
    # Sidebar
    draw.rectangle([(0, 60), (200, 600)], fill="#dddddd", outline="#aaaaaa", width=1)
    draw.text((20, 80), "Sidebar", fill="black", font=font)
    draw.text((20, 110), "- Menu Item 1", fill="#555555", font=_get_font(14))
    draw.text((20, 135), "- Menu Item 2", fill="#555555", font=_get_font(14))
    draw.text((20, 160), "- Menu Item 3", fill="#555555", font=_get_font(14))
    # Content area
    draw.rectangle([(200, 60), (800, 600)], fill="white", outline="#aaaaaa", width=1)
    draw.text((350, 280), "Main Content Area", fill="black", font=_get_font(22))
    img.save(os.path.join(ASSET_DIR, "vqa_layout.png"))


def gen_vqa_diagram() -> None:
    img = Image.new("RGB", (800, 600), "white")
    draw = ImageDraw.Draw(img)
    font = _get_font(18)

    # Boxes
    boxes = [
        ("Start", 350, 30, 100, 40),
        ("Process", 325, 140, 150, 50),
        ("Decision", 325, 270, 150, 60),
        ("End", 350, 420, 100, 40),
    ]
    for label, x, y, w, h in boxes:
        if label == "Decision":
            # Diamond shape
            cx, cy = x + w // 2, y + h // 2
            points = [(cx, y), (x + w, cy), (cx, y + h), (x, cy)]
            draw.polygon(points, outline="black", fill="#e3f2fd")
            draw.text((cx - 30, cy - 10), label, fill="black", font=font)
        elif label in ("Start", "End"):
            draw.rounded_rectangle([(x, y), (x + w, y + h)], radius=20,
                                   outline="black", fill="#c8e6c9", width=2)
            draw.text((x + 20, y + 8), label, fill="black", font=font)
        else:
            draw.rectangle([(x, y), (x + w, y + h)], outline="black",
                           fill="#fff9c4", width=2)
            draw.text((x + 30, y + 12), label, fill="black", font=font)

    # Arrows (vertical connectors)
    arrow_pairs = [(400, 70, 400, 140), (400, 190, 400, 270), (400, 330, 400, 420)]
    for x1, y1, x2, y2 in arrow_pairs:
        draw.line([(x1, y1), (x2, y2)], fill="black", width=2)
        # Arrowhead
        draw.polygon([(x2 - 6, y2 - 10), (x2 + 6, y2 - 10), (x2, y2)], fill="black")

    img.save(os.path.join(ASSET_DIR, "vqa_diagram.png"))


# ---------------------------------------------------------------------------
# Category 5: Code from Visual
# ---------------------------------------------------------------------------

def gen_code_ui_buttons() -> None:
    img = Image.new("RGB", (600, 300), "#f5f5f5")
    draw = ImageDraw.Draw(img)
    font = _get_font(18, bold=True)
    title_font = _get_font(14)

    draw.text((20, 20), "Action Panel", fill="black", font=_get_font(22, bold=True))

    buttons = [
        ("Save", "#4caf50", "white", 50),
        ("Cancel", "#ff9800", "white", 220),
        ("Delete", "#f44336", "white", 390),
    ]
    for label, bg, fg, x in buttons:
        draw.rounded_rectangle([(x, 100), (x + 140, 150)], radius=8,
                               fill=bg, outline=bg)
        draw.text((x + 35, 112), label, fill=fg, font=font)

    img.save(os.path.join(ASSET_DIR, "code_ui_buttons.png"))


def gen_code_class_diagram() -> None:
    img = Image.new("RGB", (500, 400), "white")
    draw = ImageDraw.Draw(img)
    header_font = _get_font(20, bold=True)
    font = _get_font(16)

    # UML box
    x, y, w = 50, 30, 400
    draw.rectangle([(x, y), (x + w, y + 50)], fill="#b3e5fc", outline="black", width=2)
    draw.text((x + 140, y + 12), "UserProfile", fill="black", font=header_font)

    draw.rectangle([(x, y + 50), (x + w, y + 180)], fill="white", outline="black", width=2)
    props = ["+Name : string", "+Email : string", "+Age : int"]
    py = y + 60
    for p in props:
        draw.text((x + 15, py), p, fill="black", font=font)
        py += 30

    draw.rectangle([(x, y + 180), (x + w, y + 310)], fill="white", outline="black", width=2)
    methods = ["+GetDisplayName() : string", "+IsValid() : bool", "+UpdateEmail(email : string) : void"]
    py = y + 190
    for m in methods:
        draw.text((x + 15, py), m, fill="black", font=font)
        py += 30

    img.save(os.path.join(ASSET_DIR, "code_class_diagram.png"))


# ---------------------------------------------------------------------------
# Category 6: Math
# ---------------------------------------------------------------------------

def gen_math_equation() -> None:
    fig, ax = plt.subplots(figsize=(8, 3))
    ax.text(0.5, 0.5, r"$\int_0^1 x^2\, dx = \frac{1}{3}$",
            fontsize=48, ha="center", va="center",
            transform=ax.transAxes)
    ax.axis("off")
    plt.tight_layout()
    fig.savefig(os.path.join(ASSET_DIR, "math_equation.png"), dpi=100)
    plt.close(fig)


def gen_math_expression() -> None:
    fig, ax = plt.subplots(figsize=(8, 3))
    ax.text(0.5, 0.65, r"$2x + 3y = 7$", fontsize=40, ha="center", va="center",
            transform=ax.transAxes)
    ax.text(0.5, 0.30, r"$x - y = 1$", fontsize=40, ha="center", va="center",
            transform=ax.transAxes)
    ax.axis("off")
    plt.tight_layout()
    fig.savefig(os.path.join(ASSET_DIR, "math_expression.png"), dpi=100)
    plt.close(fig)


# ---------------------------------------------------------------------------
# Runner
# ---------------------------------------------------------------------------

ALL_GENERATORS = [
    gen_ocr_code,
    gen_ocr_mixed_fonts,
    gen_ocr_numbers,
    gen_ocr_handwriting_style,
    gen_doc_invoice,
    gen_doc_table,
    gen_doc_form,
    gen_chart_bar,
    gen_chart_pie,
    gen_chart_line,
    gen_vqa_shapes,
    gen_vqa_layout,
    gen_vqa_diagram,
    gen_code_ui_buttons,
    gen_code_class_diagram,
    gen_math_equation,
    gen_math_expression,
]


def generate_all(asset_dir: str | None = None) -> None:
    global ASSET_DIR
    if asset_dir is not None:
        ASSET_DIR = asset_dir
    os.makedirs(ASSET_DIR, exist_ok=True)
    for gen_fn in ALL_GENERATORS:
        name = gen_fn.__name__.replace("gen_", "")
        print(f"  [generate] {name}.png")
        gen_fn()
    print(f"  [generate] Done — {len(ALL_GENERATORS)} images in {ASSET_DIR}")


if __name__ == "__main__":
    generate_all()
