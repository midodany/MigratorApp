import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: 'filterBR'
})

export class FilterBR implements PipeTransform {
    transform(value: any, filterString: string, propName: string): any {
        if(value.length === 0 || filterString === '') {
            return value;
        }
        const resultArray = [];
        for (const item of value) {
            if(item[propName].toLowerCase().indexOf(filterString.toLowerCase()) >= 0 ) {
                resultArray.push(item);
            }
        }
        return resultArray;
    }
}