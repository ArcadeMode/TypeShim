export function dateOnly(input: Date | number | string): Date {
  const d = input instanceof Date ? input : new Date(input);
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

export function dateOffsetHour(input: Date | number | string, hours: number): Date {
  const d = input instanceof Date ? new Date(input) : new Date(input);
  d.setHours(d.getHours() + hours);
  return d;
}