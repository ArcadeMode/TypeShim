import { describe, test, expect } from 'vitest';
import { MemoryViewMethodClass, MemoryViewPropertyClass } from "@typeshim/e2e-wasm-lib";

type AnyTypedArray = Int32Array | Uint8Array | Float64Array;
type AnyTypedArrayCtor = {
    new(length: number): AnyTypedArray;
    new(elements: ArrayLike<number>): AnyTypedArray;
    readonly BYTES_PER_ELEMENT: number;
};

type MemoryViewGetter = (instance: MemoryViewMethodClass) => {
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
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        expect(view.length).toBe(expected.length);
    });

    test(`${typeLabel} ${viewKind} byteLength is as expected`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        expect(view.byteLength).toBe(expected.length * ctor.BYTES_PER_ELEMENT);
    });

    test(`${typeLabel} ${viewKind} slice is as expected`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        expect(view.slice(0, 4)).toEqual(new ctor(expected.slice(0, 4)));
    });

    test(`${typeLabel} ${viewKind} set is as expected`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        const source = new ctor(expected.slice(1).reverse());
        view.set(source, 1);
        expect(view.slice()).toEqual(new ctor([expected[0], ...Array.from(source)]));
    });

    test(`${typeLabel} ${viewKind} set throws on out of bounds write`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        expect(() => view.set(new ctor(expected.slice(1).reverse()), 5)).toThrow("offset is out of bounds");
    });

    test(`${typeLabel} ${viewKind} copyTo works as expected`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        const target = new ctor(expected.length);
        view.copyTo(target);
        expect(target).toEqual(new ctor(expected));
    });

    test(`${typeLabel} ${viewKind} copyTo throws on out of bounds write due to array size`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        const target = new ctor(1);
        expect(() => view.copyTo(target)).toThrow("offset is out of bounds");
    });

    test(`${typeLabel} ${viewKind} copyTo copies from offset`, () => {
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        const target = new ctor(4);
        view.copyTo(target, 2);
        expect(target).toEqual(new ctor([expected[2], expected[3], expected[4], 0]));
    });

    test(`${typeLabel} ${viewKind} disposes as expected`, () => {
        const testInstance = new MemoryViewMethodClass();
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
    sum: (instance: MemoryViewMethodClass, view: ReturnType<MemoryViewGetter>) => number;
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
        const testInstance = new MemoryViewMethodClass();
        const view = getView(testInstance);
        const actual = sum(testInstance, view);
        if (typeLabel === 'Double') {
            expect(actual).toBeCloseTo(expectedSum, 10);
        } else {
            expect(actual).toBe(expectedSum);
        }
    });

    test(`${typeLabel} ${viewKind} sum reflects mutations from JS`, () => {
        const testInstance = new MemoryViewMethodClass();
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
    describe('Span  Methods', () => {
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

    describe('ArraySegment Methods', () => {
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

    describe('ArraySegment Properties', () => {
        test('Byte ArraySegment property is gettable', () => {
            const helper = new MemoryViewMethodClass();
            const arraySegment = helper.GetByteArraySegment();
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: arraySegment,
                Int32ArraySegment: helper.GetInt32ArraySegment(),
                DoubleArraySegment: helper.GetDoubleArraySegment(),
            });
            const propertyValue = testInstance.ByteArraySegment;
            expect(propertyValue).toEqual(arraySegment);
            expect(propertyValue.slice()).toEqual(arraySegment.slice());

            arraySegment.set(new Uint8Array([10, 20]), 0);
            expect(propertyValue.slice()).toEqual(arraySegment.slice()); // still equals because same memory
        });

        test('Int32 ArraySegment property is gettable', () => {
            const helper = new MemoryViewMethodClass();
            const arraySegment = helper.GetInt32ArraySegment();
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: helper.GetByteArraySegment(),
                Int32ArraySegment: arraySegment,
                DoubleArraySegment: helper.GetDoubleArraySegment(),
            });
            const propertyValue = testInstance.Int32ArraySegment;
            expect(propertyValue).toEqual(arraySegment);
            expect(propertyValue.slice()).toEqual(arraySegment.slice());

            arraySegment.set(new Int32Array([10, 20]), 0);
            expect(propertyValue.slice()).toEqual(arraySegment.slice()); // still equals because same memory
        });

        test('Double ArraySegment property is gettable', () => {
            const helper = new MemoryViewMethodClass();
            const arraySegment = helper.GetDoubleArraySegment();
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: helper.GetByteArraySegment(),
                Int32ArraySegment: helper.GetInt32ArraySegment(),
                DoubleArraySegment: arraySegment,
            });
            const propertyValue = testInstance.DoubleArraySegment;
            expect(propertyValue).toEqual(arraySegment);
            expect(propertyValue.slice()).toEqual(arraySegment.slice());

            arraySegment.set(new Float64Array([10.5, 20.5]), 0);
            expect(propertyValue.slice()).toEqual(arraySegment.slice()); // still equals because same memory
        });

        test('Byte ArraySegment property is settable', () => {
            const helper = new MemoryViewMethodClass();
            
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: helper.GetByteArraySegment(),
                Int32ArraySegment: helper.GetInt32ArraySegment(),
                DoubleArraySegment: helper.GetDoubleArraySegment(),
            });
            const arraySegment = helper.GetByteArraySegment();
            arraySegment.set(new Uint8Array([10, 20]), 0);
            testInstance.ByteArraySegment = arraySegment;
            expect(testInstance.ByteArraySegment).toEqual(arraySegment);
            expect(testInstance.ByteArraySegment.slice()).toEqual(arraySegment.slice());
        });

        test('Int32 ArraySegment property is settable', () => {
            const helper = new MemoryViewMethodClass();
            
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: helper.GetByteArraySegment(),
                Int32ArraySegment: helper.GetInt32ArraySegment(),
                DoubleArraySegment: helper.GetDoubleArraySegment(),
            });
            const arraySegment = helper.GetInt32ArraySegment();
            arraySegment.set(new Int32Array([10, 20]), 0);
            testInstance.Int32ArraySegment = arraySegment;
            expect(testInstance.Int32ArraySegment).toEqual(arraySegment);
            expect(testInstance.Int32ArraySegment.slice()).toEqual(arraySegment.slice());
        });

        test('Double ArraySegment property is settable', () => {
            const helper = new MemoryViewMethodClass();
            
            const testInstance = new MemoryViewPropertyClass({
                ByteArraySegment: helper.GetByteArraySegment(),
                Int32ArraySegment: helper.GetInt32ArraySegment(),
                DoubleArraySegment: helper.GetDoubleArraySegment(),
            });
            const arraySegment = helper.GetDoubleArraySegment();
            arraySegment.set(new Float64Array([10.5, 20.5]), 0);
            testInstance.DoubleArraySegment = arraySegment;
            expect(testInstance.DoubleArraySegment).toEqual(arraySegment);
            expect(testInstance.DoubleArraySegment.slice()).toEqual(arraySegment.slice());
        });


    });
});