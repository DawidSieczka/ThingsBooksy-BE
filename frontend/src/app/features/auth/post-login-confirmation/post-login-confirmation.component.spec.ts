import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { PostLoginConfirmationComponent } from './post-login-confirmation.component';

describe('PostLoginConfirmationComponent', () => {
  let fixture: ComponentFixture<PostLoginConfirmationComponent>;
  let component: PostLoginConfirmationComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PostLoginConfirmationComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PostLoginConfirmationComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('email', 'jane@example.com');
    fixture.detectChanges();
  });

  it('should emit signOut when invoked', () => {
    const spy = vi.fn();
    component.signOut.subscribe(spy);
    component.onSignOut();
    expect(spy).toHaveBeenCalled();
  });
});
