import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { OrderService } from './order.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule, CommonModule, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
  providers: [OrderService],
})
export class App {
  protected readonly title = signal('Frontend');
  orderForm: FormGroup;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private toastr: ToastrService,
  ) {
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
      price: ['', [Validators.required, Validators.min(0.01), Validators.max(999.99)]],
    });
  }

  selectSide(side: string) {
    this.orderForm.patchValue({ side });
  }

  onQuantityInput(event: any) {
    const input = event.target;
    let digits = this.formatInteger(input.value);
    this.orderForm.get('quantity')?.setValue(digits);

    // Se o campo estiver vazio, limpa erros de validação customizada
    if (!digits) {
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
      return;
    }

    // Validações
    const numValue = parseInt(digits);

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
    let digits = this.formatInteger(input.value);

    if (!digits) {
      input.value = '';
      this.orderForm.get('price')?.setValue('');

      // Limpa erros se o campo estiver vazio
      const errors = this.orderForm.get('price')?.errors;
      if (errors) {
        delete errors['max'];
        delete errors['min'];

        if (Object.keys(errors).length === 0) {
          this.orderForm.get('price')?.setErrors(null);
        } else {
          this.orderForm.get('price')?.setErrors(errors);
        }
      }
      return;
    }

    // Converte para centavos e formata
    const cents = parseInt(digits, 10);
    const reais = Math.floor(cents / 100);
    const centavos = cents % 100;

    // Formata o valor
    const formattedValue = `${reais},${centavos.toString().padStart(2, '0')}`;

    // Atualiza o campo
    input.value = formattedValue;

    // Atualiza o form control com o valor real (em reais com ponto)
    const realValue = (cents / 100).toFixed(2);
    this.orderForm.get('price')?.setValue(realValue);

    // Coloca o cursor no final
    setTimeout(() => {
      input.setSelectionRange(formattedValue.length, formattedValue.length);
    }, 0);

    // VALIDAÇÃO CORRETA - usa realValue, não digits!
    const numValue = parseFloat(realValue);

    // Limpa erros anteriores
    const currentErrors: any = {};

    // Verifica máximo
    if (numValue > 999.99) {
      currentErrors['max'] = true;
    }

    // Verifica mínimo
    if (numValue < 0.01) {
      currentErrors['min'] = true;
    }

    // Verifica se o campo está preenchido (required)
    if (!realValue) {
      currentErrors['required'] = true;
    }

    // Aplica os erros se houver algum, senão limpa
    if (Object.keys(currentErrors).length > 0) {
      this.orderForm.get('price')?.setErrors(currentErrors);
    } else {
      this.orderForm.get('price')?.setErrors(null);
    }
  }

  formatInteger(value: any): string {
    // Remove caracteres não numéricos
    let result = value.replace(/[^0-9]/g, '');
    return result;
  }

  onSubmit() {
    if (!this.orderForm.valid) {
      this.toastr.warning('Preencha todos os campos corretamente', 'Formulário Inválido');
      return;
    }

    this.submitted = true;
    this.createOrder();
    console.log('Ordem enviada:', this.orderForm.value);
  }

  createOrder() {
    const formValue = this.orderForm.value;

    const orderToSend = {
      symbol: formValue.symbol.toUpperCase(),
      side: formValue.side.toUpperCase(),
      quantity: parseInt(formValue.quantity),
      price: parseFloat(formValue.price),
    };

    this.orderService.createOrder(orderToSend).subscribe({
      next: (response) => {
        console.log('Ordem criada com sucesso:', response);
        // alert('Ordem criada com sucesso!');
        this.toastr.success('Ordem criada com sucesso!', 'Sucesso');
        this.orderForm.reset(
          { side: 'COMPRA' },
          {
            emitEvent: false,
            onlySelf: true,
          },
        );

        this.submitted = false;
      },
      error: (error) => {
        console.error('Erro ao criar ordem:', error);

        if (error.status === 0) {
          this.toastr.warning(
            'Erro: A API está indisponível. Verifique se o servidor está rodando.',
          );
        } else if (error.status === 404) {
          this.toastr.error('Erro 404: Endpoint não encontrado.', 'Erro');
        } else if (error.status === 500) {
          this.toastr.error('Erro interno do servidor. Tente novamente mais tarde.', 'Erro');
        } else if (error.status === 400 && Array.isArray(error.error?.errors)) {
          const errorList = error.error.errors.join('<br/><br/>');
          this.toastr.error(
            `Erro 400: Requisição Inválida. Erros de validação:<br/><br/>${errorList}`,
            'Erro',
            { enableHtml: true },
          );
        } else {
          this.toastr.error(`Erro ao criar ordem: ${error.message}`, 'Erro', { enableHtml: true });
        }
      },
    });
  }

  getValidSymbols() {
    this.orderService.getValidSymbols().subscribe({
      next: (symbols) => {
        console.log('Símbolos recebidos:', symbols);
      },
      error: (error) => {
        console.error('Erro ao consultar Símbolos:', error);

        if (error.status === 0) {
          alert('Erro: A API está indisponível. Verifique se o servidor está rodando.');
        } else if (error.status === 404) {
          alert('Erro 404: Endpoint não encontrado.');
        } else if (error.status === 500) {
          alert('Erro interno do servidor. Tente novamente mais tarde.');
        } else {
          alert(`Erro ao buscar símbolos: ${error.message}`);
        }
      },
    });
  }

  get f() {
    return this.orderForm.controls;
  }
}
