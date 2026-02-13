import { describe, test, expect } from 'vitest';
import { MemoryViewClass } from "@typeshim/e2e-wasm-lib";

describe('MemoryView Tests', () => {

    test('Span length is as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        expect(span.length).toBe(5);
    });

    test('Span byteLength is as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        expect(span.byteLength).toBe(20);
    });

    test('Span slice is as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        expect(span.slice(0, 4)).toEqual(new Int32Array([0, 1, 2, 3]));
    });

    test('Span set is as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        span.set(new Int32Array([4, 3, 2, 1]), 1);
        expect(span.slice()).toEqual(new Int32Array([0, 4, 3, 2, 1]));
    });

    test('Span set throws on out of bounds write', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        expect(() => span.set(new Int32Array([4, 3, 2, 1]), 5)).toThrow("offset is out of bounds");
    });

    test('Span copyTo works as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        const newArray = new Int32Array(5);
        span.copyTo(newArray);
        expect(newArray).toEqual(new Int32Array([0, 1, 2, 3, 4]));
    });

    test('Span copyTo throws on out of bounds write due to array size', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        const newArray = new Int32Array(1); // Too small to hold the data
        expect(() => span.copyTo(newArray)).toThrow("offset is out of bounds");
    });

    test('Span copyTo copies from offset', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        const newArray = new Int32Array(4); // Big enough, but offset will be out of bounds
        span.copyTo(newArray, 2); // Start copying from index 2 of the span (3 left to copy)
        expect(newArray).toEqual(new Int32Array([2, 3, 4, 0]));
    });

    test('Span disposes as expected', () => {
        const testInstance = new MemoryViewClass();
        const span = testInstance.GetInt32Span();
        expect(span.isDisposed).toBe(false);
        span.dispose();
        expect(span.isDisposed).toBe(true);
        expect(() => span.length).toThrow('ObjectDisposedException');
    });
});