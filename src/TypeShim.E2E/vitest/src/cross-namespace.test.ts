import { describe, test, expect } from 'vitest';
import { SpikeA, SpikeB } from 'typeshim';

describe('Cross Namespace References', () => {
    test('Constructs a class referencing a class in another namespace', () => {
        const partner = new SpikeB({ Name: 'partner' });
        const a = new SpikeA({ Id: 7, Partner: partner });
        expect(a.Id).toBe(7);
        expect(a.Partner).toBeInstanceOf(SpikeB);
        expect(a.Partner.Name).toBe('partner');
    });

    test('Invokes a method that consumes the cross-namespace reference', () => {
        const a = new SpikeA({ Id: 3, Partner: new SpikeB({ Name: 'x' }) });
        expect(a.Describe()).toBe('3:x');
    });

    test('Mutates the cross-namespace reference property', () => {
        const a = new SpikeA({ Id: 1, Partner: new SpikeB({ Name: 'old' }) });
        a.Partner = new SpikeB({ Name: 'new' });
        expect(a.Partner).toBeInstanceOf(SpikeB);
        expect(a.Partner.Name).toBe('new');
    });
});
