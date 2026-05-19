import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app';
import { NavbarComponent } from './components/navbar/navbar';

describe('AppComponent', () => {

  beforeEach(async () => {

    await TestBed.configureTestingModule({
      imports: [
        AppComponent,
        RouterTestingModule
      ]
    }).compileComponents();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should create the app', () => {

    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;

    expect(app).toBeTruthy();
  });

  // ─── Template structure ───────────────────────────────────────────────────────

  it('should contain app-navbar', () => {

    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('app-navbar')).not.toBeNull();
  });

  it('should contain router-outlet', () => {

    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.querySelector('router-outlet')).not.toBeNull();
  });

});