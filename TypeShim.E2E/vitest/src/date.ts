export function dateOnly(input: Date): Date {
  return new Date(input.getFullYear(), input.getMonth(), input.getDate());
}

export function dateOffsetHour(input: Date, hours: number): Date {
  input.setHours(input.getHours() + hours);
  return input;
}