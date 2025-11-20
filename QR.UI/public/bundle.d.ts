import React from 'react';

interface PersonName {
    Value: string;
}
interface Dog {
    Name: string;
    Age: number;
}
interface Person {
    Name: PersonName;
    Age: number;
    Pet: Dog;
}

declare function generate(text: string, pixelsPerBlock: number): Promise<string>;
declare function GetPerson(name: PersonName): Promise<Person>;
type QrImageProps = {
    text?: string;
    relativePath?: string;
};
declare const QrImage: React.FC<QrImageProps>;

export { GetPerson, QrImage, generate };
