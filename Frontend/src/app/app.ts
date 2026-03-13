import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ReactiveFormsModule, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Frontend');
  orderForm: FormGroup;
  submitted = false;

  constructor(private fb: FormBuilder) {
    this.orderForm = this.fb.group({
      symbol: ['', [Validators.required, Validators.minLength(1)]],
      side: ['COMPRA', Validators.required],
      quantity: [
        '',
        [
          Validators.required,
          Validators.min(1),
          Validators.max(99999),
          Validators.pattern('^[0-9]*$'),
        ],
      ],
      price: [
        '',
        [Validators.required, Validators.min(0.01), Validators.pattern('^[0-9]+(\\.[0-9]{1,2})?$')],
      ],
    });
  }

  selectSide(side: string) {
    this.orderForm.patchValue({ side });
  }

  onQuantityInput(event: any) {
    const input = event.target;
    let value = this.formatInteger(input.value);
    this.orderForm.get('quantity')?.setValue(value);

    // Se o campo estiver vazio, limpa todos os erros (mantendo só required para submit)
    if (!value) {
      this.orderForm.get('quantity')?.setErrors(null);
      return;
    }

    // Validações
    const numValue = parseInt(value);

    // Valor máximo de 99999
    if (numValue > 99999) {
      this.orderForm.get('quantity')?.setErrors({ max: true });
      return;
    }

    // Valor mínimo de 1
    if (numValue < 1) {
      this.orderForm.get('quantity')?.setErrors({ min: true });
      return;
    }

    // Remove erros se o valor for válido
    const errors = this.orderForm.get('quantity')?.errors;
    if (errors) {
      this.orderForm.get('quantity')?.setErrors(null);
    }
  }

  onPriceInput(event: any) {
    const input = event.target;
    let value = this.formatInteger(input.value);
    this.orderForm.get('price')?.setValue(value);

    // Se o campo estiver vazio, limpa todos os erros (mantendo só required para submit)
    if (!value) {
      this.orderForm.get('price')?.setErrors(null);
      return;
    }

    // Validação em tempo real apenas se houver valor
    const numValue = parseFloat(value);
    if (numValue > 999.99) {
      this.orderForm.get('price')?.setErrors({ max: true });
      return;
    }

    if (numValue < 0.01) {
      this.orderForm.get('price')?.setErrors({ min: true });
      return;
    }

    // Remove erros se o valor for válido
    const errors = this.orderForm.get('price')?.errors;
    if (errors) {
      this.orderForm.get('price')?.setErrors(null);
    }
  }

  formatInteger(value: any): string {
    // Remove qualquer caractere que não seja número ou ponto
    let result = value.replace(/[^0-9.]/g, '');
    return result;
  }

  onSubmit() {
    this.submitted = true;
    if (this.orderForm.valid) {
      console.log('Ordem enviada:', this.orderForm.value);
      // Enviar dados para o backend
      // this.orderService.createOrder(this.orderForm.value).subscribe(...);
    }
  }

  get f() {
    return this.orderForm.controls;
  }
}
