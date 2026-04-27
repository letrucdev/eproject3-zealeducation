import { CurrencyPipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'vnd' })
export class VndPipe implements PipeTransform {
  private readonly _pipe = new CurrencyPipe('vi-VN', 'VND');

  transform(value: number | string | null | undefined): string | null {
    return this._pipe.transform(value, 'VND', 'symbol', '1.0-0', 'vi-VN');
  }
}
