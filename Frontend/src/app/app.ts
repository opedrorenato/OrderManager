import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ReactiveFormsModule, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Frontend');
  orderForm: FormGroup;
  submitted = false;

  constructor(private fb: FormBuilder) {
    this.orderForm = this.fb.group({
      symbol: ['', [Validators.required, Validators.minLength(1)]],
      side: ['COMPRA', Validators.required],
      quantity: ['', [Validators.required, Validators.min(1), Validators.max(99999), Validators.pattern('^[0-9]*$')]],
      price: ['', [Validators.required, Validators.min(0.01), Validators.pattern('^[0-9]+(\\.[0-9]{1,2})?$')]]
    });
  }

  selectSide(side: string) {
    this.orderForm.patchValue({ side });
  }

  onQuantityInput(event: any) {
    const input = event.target;
    let value = input.value;

    // Remove qualquer caractere que não seja número
    value = value.replace(/[^0-9]/g, '');

    // Atualiza o valor no form control
    this.orderForm.get('quantity')?.setValue(value);

    // Se o campo estiver vazio, limpa todos os erros (mantendo só required para submit)
    if (!value) {
      this.orderForm.get('quantity')?.setErrors(null);
      return;
    }

    // Validação em tempo real apenas se houver valor
    const numValue = parseInt(value);
    if (numValue > 99999) {
      this.orderForm.get('quantity')?.setErrors({ max: true });
    } else if (numValue < 1) {
      this.orderForm.get('quantity')?.setErrors({ min: true });
    } else {
      // Remove erros se o valor for válido
      const errors = this.orderForm.get('quantity')?.errors;
      if (errors) {
        delete errors['max'];
        delete errors['min'];
        if (Object.keys(errors).length === 0) {
          this.orderForm.get('quantity')?.setErrors(null);
        } else {
          this.orderForm.get('quantity')?.setErrors(errors);
        }
      }
    }
  }

  onSubmit() {
    this.submitted = true;
    if (this.orderForm.valid) {
      console.log('Formulário enviado:', this.orderForm.value);
      // Aqui você pode enviar os dados para o backend
      // this.orderService.createOrder(this.orderForm.value).subscribe(...);
    }
  }

  get f() {
    return this.orderForm.controls;
  }
}
