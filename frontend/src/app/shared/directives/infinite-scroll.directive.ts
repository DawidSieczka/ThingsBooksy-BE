import {
  AfterViewInit,
  Directive,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  Renderer2,
  inject,
} from '@angular/core';

@Directive({
  selector: '[tbInfiniteScroll]',
  standalone: true,
})
export class InfiniteScrollDirective implements AfterViewInit, OnDestroy {
  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly renderer = inject(Renderer2);

  @Input({ alias: 'tbInfiniteScrollDisabled' }) disabled = false;
  @Input({ alias: 'tbInfiniteScrollRootMargin' }) rootMargin = '120px';
  @Output() loadMore = new EventEmitter<void>();

  private sentinel?: HTMLElement;
  private observer?: IntersectionObserver;

  ngAfterViewInit(): void {
    this.sentinel = this.renderer.createElement('div') as HTMLElement;
    this.renderer.setAttribute(this.sentinel, 'aria-hidden', 'true');
    this.renderer.setStyle(this.sentinel, 'height', '1px');
    this.renderer.setStyle(this.sentinel, 'width', '100%');
    this.renderer.appendChild(this.host.nativeElement, this.sentinel);

    this.observer = new IntersectionObserver(
      entries => {
        if (this.disabled) {
          return;
        }
        if (entries.some(e => e.isIntersecting)) {
          this.loadMore.emit();
        }
      },
      { root: null, rootMargin: this.rootMargin, threshold: 0 },
    );
    this.observer.observe(this.sentinel);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.sentinel?.parentNode) {
      this.renderer.removeChild(this.sentinel.parentNode, this.sentinel);
    }
  }
}
