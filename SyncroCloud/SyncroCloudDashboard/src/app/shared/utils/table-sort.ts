import { Sort } from '@angular/material/sort';

/**
 * Returns a new array sorted according to a MatSort state, using a per-column
 * value accessor. When no column is active (or direction is cleared) the input
 * order is preserved. Designed for the zoneless signal + `computed()` pattern:
 * call it inside a `computed()` that reads the source-data signal and a
 * `signal<Sort>` so sorting re-evaluates reactively.
 */
export function sortRows<T>(
  data: readonly T[],
  sort: Sort,
  accessor: (row: T, column: string) => unknown
): T[] {
  const rows = [...data];
  if (!sort.active || sort.direction === '') return rows;

  const factor = sort.direction === 'asc' ? 1 : -1;
  return rows.sort((a, b) => compareValues(accessor(a, sort.active), accessor(b, sort.active)) * factor);
}

function compareValues(a: unknown, b: unknown): number {
  const aNil = a === null || a === undefined || a === '';
  const bNil = b === null || b === undefined || b === '';
  if (aNil && bNil) return 0;
  if (aNil) return 1;   // empty values always sort to the bottom
  if (bNil) return -1;

  if (typeof a === 'number' && typeof b === 'number') return a - b;
  if (a instanceof Date && b instanceof Date) return a.getTime() - b.getTime();

  return String(a).localeCompare(String(b), undefined, { numeric: true, sensitivity: 'base' });
}
