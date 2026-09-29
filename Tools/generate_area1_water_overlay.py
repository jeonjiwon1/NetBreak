"""Generate the seamless, low-contrast Area 1 pixel caustics sprite."""

from pathlib import Path

import numpy as np
from PIL import Image


SIZE = 256
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/Resources/Area1/CoastWaterCaustics.png"


def main() -> None:
    rng = np.random.default_rng(1917)
    seeds = rng.uniform(0, SIZE, (22, 2))
    y, x = np.mgrid[:SIZE, :SIZE].astype(np.float32)
    angle = 2 * np.pi / SIZE
    warped_x = x + 7 * np.sin(2 * angle * y) + 3 * np.sin(angle * (x + y))
    warped_y = y + 6 * np.sin(2 * angle * x) - 3 * np.sin(angle * (x - y))

    nearest = np.full((SIZE, SIZE), np.inf, dtype=np.float32)
    second = nearest.copy()
    for seed_x, seed_y in seeds:
        dx = np.abs(warped_x - seed_x) % SIZE
        dy = np.abs(warped_y - seed_y) % SIZE
        dx = np.minimum(dx, SIZE - dx)
        dy = np.minimum(dy, SIZE - dy)
        distance = dx * dx + dy * dy
        second = np.minimum(second, np.maximum(nearest, distance))
        nearest = np.minimum(nearest, distance)

    # Broad, irregular cells with restrained light at their edges.
    edge = (np.sqrt(second) - np.sqrt(nearest))
    alpha = np.where(edge < 2.5, 76, np.where(edge < 5, 50, np.where(edge < 8, 24, 0)))
    pixels = np.empty((SIZE, SIZE, 4), dtype=np.uint8)
    pixels[:, :, :3] = (219, 252, 250)
    pixels[:, :, 3] = alpha.astype(np.uint8)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(pixels, "RGBA").save(OUTPUT)


if __name__ == "__main__":
    main()
