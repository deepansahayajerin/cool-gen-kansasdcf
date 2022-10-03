export interface Rect
{
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface ItemInfo extends Rect
{
  index?: number;
  name?: string;
  col?: string;
  row?: string;
  tabindex?: number;
  readonly?: boolean;
  disabled?: boolean;
  value?: string;
}

export enum Quadrant
{
  None = 0,
  North = 1,
  South = 2,
  West = 4,
  East = 8,
  Middle = 16,
  NorthWest = 32,
  NorthEast = 64,
  SouthWest = 128,
  SouthEast = 256,
  Closest = 512,

  Horizontal = West | Middle | East,
  Vertical = North | Middle | South,
  Up = NorthWest | North | NorthEast,
  Down = SouthWest | South | SouthEast,
  Left = NorthWest | West | SouthWest,
  Right = NorthEast | East | SouthEast,
  All = Up | Horizontal | Down,
}

export type Metric =
{
  item: ItemInfo;
  quadrants: Quadrant;
  dx: number;
  dy: number;
}

export function distanceMetric(
  from: Rect, 
  to: Rect,
  epsilon: number): Metric
{
  const ex = to.width * epsilon;
  const ey = to.height * epsilon;

  const left = to.x + ex < from.x;

  const center = to.x + ex < from.x ?
    to.x + to.width > from.x + ex :
    to.x + ex < from.x + from.width;

  const right = to.x + ex < from.x ?
    (to.x + to.width > from.x + ex) &&
      (to.x + to.width > from.x + from.width + ex) : 
    (to.x + ex >= from.x + from.width) || 
      (to.x + to.width > from.x + from.width + ex);

  const up = to.y + ey < from.y;

  const middle = to.y + ey < from.y ?
    to.y + to.height > from.y + ey :
    to.y + ey < from.y + from.height;

  const down = to.y + ey < from.y ?
    (to.y + to.height > from.y + ey) && 
      (to.y + to.height > from.y + from.height + ey) :
    (to.y + ey >= from.y + from.height) ||
      (to.y + to.height > from.y + from.height + ey);

  let quadrants: Quadrant = Quadrant.None;

  if (left)
  {
    if (up) 
    {
      quadrants |= Quadrant.NorthWest;
    }

    if (middle) 
    {
      quadrants |= Quadrant.West;
    }

    if (down) 
    {
      quadrants |= Quadrant.SouthWest;
    }
  }

  if (center)
  {
    if (up) 
    {
      quadrants |= Quadrant.North;
    }

    if (middle) 
    {
      quadrants |= Quadrant.Middle;
    }
  
    if (down) 
    {
      quadrants |= Quadrant.South;
    }
  }

  if (right)
  {
    if (up) 
    {
      quadrants |= Quadrant.NorthEast;
    }

    if (middle) 
    {
      quadrants |= Quadrant.East;
    }

    if (down) 
    {
      quadrants |= Quadrant.SouthEast;
    }
  }

  const dx = from.x > to.x + to.width ? from.x - to.x - to.width :
    from.x + from.width < to.x ? to.x - from.x - from.width : 0;
  const dy = from.y > to.y + to.height ? from.y - to.y - to.height :
    from.y + from.height < to.y ? to.y - from.y - from.height : 0;

  return { quadrants, dx, dy, item: to };
}

export function matchesMetric(
  metric: Metric, 
  quadrant: Quadrant): boolean
{
  return !!(metric.quadrants & quadrant & Quadrant.All);
}

export function compareMetrics(
  first: Metric,
  second: Metric,
  options:
  {
      quadrant: Quadrant,
      error?: number,
      rect?: Rect,
      anchorRect?: Rect
  }): number
{ 
  const firstQuadrants = first.quadrants & options.quadrant & Quadrant.All;
  const secondQuadrants = second.quadrants & options.quadrant & Quadrant.All;

  if (!firstQuadrants !== !secondQuadrants)
  {
    return firstQuadrants ? -1 : 1;
  }
  
  if (!(options.quadrant & Quadrant.Closest))
  {
    if (firstQuadrants !== secondQuadrants)
    {
      if (firstQuadrants & Quadrant.East)
      {
        if (!(secondQuadrants & Quadrant.East))
        {
          return -1;
        }
      }
      else
      {
        if (secondQuadrants & Quadrant.East)
        {
          return 1;
        }
      }

      if (firstQuadrants & Quadrant.West)
      {
        if (!(secondQuadrants & Quadrant.West))
        {
          return -1;
        }
      }
      else
      {
        if (secondQuadrants & Quadrant.West)
        {
          return 1;
        }
      }
    
      if (firstQuadrants & Quadrant.South)
      {
        if (!(secondQuadrants & Quadrant.South))
        {
          return -1;
        }
      }
      else
      {
        if (secondQuadrants & Quadrant.South)
        {
          return 1;
        }
      }

      if (firstQuadrants & Quadrant.North)
      {
        if (!(secondQuadrants & Quadrant.North))
        {
          return -1;
        }
      }
      else
      {
        if (secondQuadrants & Quadrant.North)
        {
          return 1;
        }
      }
    }
  }

  const rect = options.rect;
  const anchorRect = options.anchorRect;

  if (anchorRect && rect)
  {
    const r1 = first.item;

    const s1 = (rect.x - r1.x + (rect.width - r1.width) / 2) *
      (anchorRect.y - r1.y + (anchorRect.height - r1.height) / 2) -
      (rect.y - r1.y + (rect.height - r1.height) / 2) *
      (anchorRect.x - r1.x + (anchorRect.width - r1.width) / 2);

    const r2 = second.item;

    const s2 = (rect.x - r2.x + (rect.width - r2.width) / 2) *
      (anchorRect.y - r2.y + (anchorRect.height - r2.height) / 2) -
      (rect.y - r2.y + (rect.height - r2.height) / 2) *
      (anchorRect.x - r2.x + (anchorRect.width - r2.width) / 2);

    if (s1 > 0)
    {
      if (s2 < 0)
      {
        return -1;
      }
    }
    else
    {
      if (s2 > 0)
      {
        return 1;
      }
    }
  }
  
  const error = options.error;
  
  const fdx = !(options.quadrant & (Quadrant.Left | Quadrant.Right)) ? 0 :
    error > 0 ? first.dx - first.dx % error : first.dx;
  const fdy = !(options.quadrant & (Quadrant.Up | Quadrant.Down)) ? 0 :
    error > 0 ? first.dy - first.dy % error : first.dy;
  const sdx = !(options.quadrant & (Quadrant.Left | Quadrant.Right)) ? 0 :
    error > 0 ? second.dx - second.dx % error : second.dx;
  const sdy = !(options.quadrant & (Quadrant.Up | Quadrant.Down)) ? 0 :
    error > 0 ? second.dy - second.dy % error : second.dy;

  // const delta = Math.hypot(fdx / first.item.width, fdy / first.item.height) -
  //   Math.hypot(sdx / second.item.width, sdy / second.item.height);

  // const delta = Math.hypot(fdx, fdy) - Math.hypot(sdx, sdy);

  // if (delta != 0)
  // {
  //   return delta;
  // }

  const dy = Math.abs(fdy) - Math.abs(sdy);

  if (dy != 0)
  {
    return dy;
  }

  const dx = Math.abs(fdx) - Math.abs(sdx);

  if (dx != 0)
  {
    return dy;
  }

  const tabIndex1 = first.item.tabindex ?? 0;
  const tabIndex2 = second.item.tabindex ?? 0;

  if (tabIndex1 !== tabIndex2)
  {
    if (tabIndex1 < 0)
    {
      if (tabIndex2 >= 0)
      {
        return 1;
      }
    }
    else
    {
      if (tabIndex2 < 0)
      {
        return -1;
      }

      return tabIndex1 - tabIndex2;
    }
  }

  const deltaY = error > 0 ?
    (first.item.y - first.item.y % error) -
    (second.item.y - second.item.y % error) :
    first.item.y - second.item.y;

  if (deltaY != 0)
  {
    return deltaY;
  }

  const deltaX = error > 0 ?
    (first.item.x - first.item.x % error) -
    (second.item.x - second.item.x % error) :
    first.item.x - second.item.x;

  if (deltaX != 0)
  {
    return deltaX;
  }

  return (first.item.index ?? 0) - (second.item.index ?? 0);
}