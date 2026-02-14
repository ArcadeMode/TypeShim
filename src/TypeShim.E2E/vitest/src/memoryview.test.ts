import { describe, test, expect } from 'vitest';
import { MemoryViewClass } from "@typeshim/e2e-wasm-lib";

type AnyTypedArray = Int32Array | Uint8Array | Float64Array;
type AnyTypedArrayCtor = {
    new(length: number): AnyTypedArray;
    new(elements: ArrayLike<number>): AnyTypedArray;
    readonly BYTES_PER_ELEMENT: number;
};

type MemoryViewGetter = (instance: MemoryViewClass) => {
    set(source: AnyTypedArray, targetOffset?: number): void;
    copyTo(target: AnyTypedArray, sourceOffset?: number): void;
    slice(start?: number, end?: number): AnyTypedArray;
    readonly length: number;
    readonly byteLength: number;
    readonly isDisposed: boolean;
    dispose(): void;
};

function runMemoryViewTests(options: {
    viewKind: 'Span' | 'ArraySegment';
    typeLabel: 'Byte' | 'Int32' | 'Double';
    getView: MemoryViewGetter;
    ctor: AnyTypedArrayCtor;
    expected: number[];
}) {
    const { viewKind, typeLabel, getView, ctor, expected } = options;

    test(`${typeLabel} ${viewKind} length is as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        expect(view.length).toBe(expected.length);
    });

    test(`${typeLabel} ${viewKind} byteLength is as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        expect(view.byteLength).toBe(expected.length * ctor.BYTES_PER_ELEMENT);
    });

    test(`${typeLabel} ${viewKind} slice is as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        expect(view.slice(0, 4)).toEqual(new ctor(expected.slice(0, 4)));
    });

    test(`${typeLabel} ${viewKind} set is as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const source = new ctor(expected.slice(1).reverse());
        view.set(source, 1);
        expect(view.slice()).toEqual(new ctor([expected[0], ...Array.from(source)]));
    });

    test(`${typeLabel} ${viewKind} set throws on out of bounds write`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        expect(() => view.set(new ctor(expected.slice(1).reverse()), 5)).toThrow("offset is out of bounds");
    });

    test(`${typeLabel} ${viewKind} copyTo works as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const target = new ctor(expected.length);
        view.copyTo(target);
        expect(target).toEqual(new ctor(expected));
    });

    test(`${typeLabel} ${viewKind} copyTo throws on out of bounds write due to array size`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const target = new ctor(1);
        expect(() => view.copyTo(target)).toThrow("offset is out of bounds");
    });

    test(`${typeLabel} ${viewKind} copyTo copies from offset`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const target = new ctor(4);
        view.copyTo(target, 2);
        expect(target).toEqual(new ctor([expected[2], expected[3], expected[4], 0]));
    });

    test(`${typeLabel} ${viewKind} disposes as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        expect(view.isDisposed).toBe(false);
        view.dispose();
        expect(view.isDisposed).toBe(true);
        expect(() => view.length).toThrow('ObjectDisposedException');
    });
}

function runMemoryViewSumTests(options: {
    viewKind: 'Span' | 'ArraySegment';
    typeLabel: 'Byte' | 'Int32' | 'Double';
    getView: MemoryViewGetter;
    sum: (instance: MemoryViewClass, view: ReturnType<MemoryViewGetter>) => number;
    expectedSum: number;
    setValues: number[];
    expectedSumAfterSet: number;
}) {
    const {
        viewKind,
        typeLabel,
        getView,
        sum,
        expectedSum,
        setValues,
        expectedSumAfterSet,
    } = options;

    test(`${typeLabel} ${viewKind} sum is as expected`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const actual = sum(testInstance, view);
        if (typeLabel === 'Double') {
            expect(actual).toBeCloseTo(expectedSum, 10);
        } else {
            expect(actual).toBe(expectedSum);
        }
    });

    test(`${typeLabel} ${viewKind} sum reflects mutations from JS`, () => {
        const testInstance = new MemoryViewClass();
        const view = getView(testInstance);
        const ctor = (view.slice().constructor as unknown) as AnyTypedArrayCtor;
        view.set(new ctor(setValues));

        const actual = sum(testInstance, view);
        if (typeLabel === 'Double') {
            expect(actual).toBeCloseTo(expectedSumAfterSet, 10);
        } else {
            expect(actual).toBe(expectedSumAfterSet);
        }
    });
}

describe('MemoryView Tests', () => {
    describe('Span', () => {
        runMemoryViewTests({
            viewKind: 'Span',
            typeLabel: 'Int32',
            getView: (x) => x.GetInt32Span(),
            ctor: Int32Array,
            expected: [0, 1, 2, 3, 4],
        });

        runMemoryViewTests({
            viewKind: 'Span',
            typeLabel: 'Byte',
            getView: (x) => x.GetByteSpan(),
            ctor: Uint8Array,
            expected: [1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4],
        });

        runMemoryViewTests({
            viewKind: 'Span',
            typeLabel: 'Double',
            getView: (x) => x.GetDoubleSpan(),
            ctor: Float64Array,
            expected: [0.1, 1.1, 2.1, 3.1, 4.1],
        });

        describe('Sum', () => {
            runMemoryViewSumTests({
                viewKind: 'Span',
                typeLabel: 'Int32',
                getView: (x) => x.GetInt32Span(),
                sum: (x, v) => x.SumInt32Span(v as any),
                expectedSum: 10,
                setValues: [10, 20, 30, 40, 50],
                expectedSumAfterSet: 150,
            });

            runMemoryViewSumTests({
                viewKind: 'Span',
                typeLabel: 'Byte',
                getView: (x) => x.GetByteSpan(),
                sum: (x, v) => x.SumByteSpan(v as any),
                expectedSum: (1 << 0) + (1 << 1) + (1 << 2) + (1 << 3) + (1 << 4),
                setValues: [10, 20, 30, 40, 50],
                expectedSumAfterSet: 150,
            });

            runMemoryViewSumTests({
                viewKind: 'Span',
                typeLabel: 'Double',
                getView: (x) => x.GetDoubleSpan(),
                sum: (x, v) => x.SumDoubleSpan(v as any),
                expectedSum: 0.1 + 1.1 + 2.1 + 3.1 + 4.1,
                setValues: [10.5, 20.5, 30.5, 40.5, 50.5],
                expectedSumAfterSet: 152.5,
            });
        });
    });

    describe('ArraySegment', () => {
        runMemoryViewTests({
            viewKind: 'ArraySegment',
            typeLabel: 'Int32',
            getView: (x) => x.GetInt32ArraySegment(),
            ctor: Int32Array,
            expected: [0, 1, 2, 3, 4],
        });

        runMemoryViewTests({
            viewKind: 'ArraySegment',
            typeLabel: 'Byte',
            getView: (x) => x.GetByteArraySegment(),
            ctor: Uint8Array,
            expected: [1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4],
        });

        runMemoryViewTests({
            viewKind: 'ArraySegment',
            typeLabel: 'Double',
            getView: (x) => x.GetDoubleArraySegment(),
            ctor: Float64Array,
            expected: [0.1, 1.1, 2.1, 3.1, 4.1],
        });

        describe('Sum', () => {
            runMemoryViewSumTests({
                viewKind: 'ArraySegment',
                typeLabel: 'Int32',
                getView: (x) => x.GetInt32ArraySegment(),
                sum: (x, v) => x.SumInt32ArraySegment(v as any),
                expectedSum: 10,
                setValues: [10, 20, 30, 40, 50],
                expectedSumAfterSet: 150,
            });

            runMemoryViewSumTests({
                viewKind: 'ArraySegment',
                typeLabel: 'Byte',
                getView: (x) => x.GetByteArraySegment(),
                sum: (x, v) => x.SumByteArraySegment(v as any),
                expectedSum: (1 << 0) + (1 << 1) + (1 << 2) + (1 << 3) + (1 << 4),
                setValues: [10, 20, 30, 40, 50],
                expectedSumAfterSet: 150,
            });

            runMemoryViewSumTests({
                viewKind: 'ArraySegment',
                typeLabel: 'Double',
                getView: (x) => x.GetDoubleArraySegment(),
                sum: (x, v) => x.SumDoubleArraySegment(v as any),
                expectedSum: 0.1 + 1.1 + 2.1 + 3.1 + 4.1,
                setValues: [10.5, 20.5, 30.5, 40.5, 50.5],
                expectedSumAfterSet: 152.5,
            });
        });
    });
});