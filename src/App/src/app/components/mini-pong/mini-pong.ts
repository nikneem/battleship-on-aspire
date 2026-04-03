import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  NgZone,
  viewChild,
  ElementRef
} from '@angular/core';

const W = 180;
const H = 240;
const PADDLE_W = 50;
const PADDLE_H = 8;
const PADDLE_MARGIN = 14;
const BALL_R = 5;
const BASE_SPEED = 2.8;
const AI_SPEED = 2.2;

interface GameState {
  ballX: number;
  ballY: number;
  ballDx: number;
  ballDy: number;
  playerX: number;
  aiX: number;
  playerScore: number;
  aiScore: number;
}

function randomSign(): 1 | -1 {
  return Math.random() > 0.5 ? 1 : -1;
}

@Component({
  selector: 'bat-mini-pong',
  templateUrl: './mini-pong.html',
  styleUrl: './mini-pong.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MiniPongComponent implements AfterViewInit {
  private readonly zone = inject(NgZone);
  private readonly destroyRef = inject(DestroyRef);
  private readonly canvasRef = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private rafId: number | null = null;
  private state!: GameState;

  ngAfterViewInit(): void {
    const canvas = this.canvasRef().nativeElement;
    this.setupHiDpi(canvas);
    this.resetState();

    this.zone.runOutsideAngular(() => {
      this.attachInputHandlers(canvas);
      this.loop(canvas);
    });

    this.destroyRef.onDestroy(() => {
      if (this.rafId !== null) {
        cancelAnimationFrame(this.rafId);
        this.rafId = null;
      }
    });
  }

  private setupHiDpi(canvas: HTMLCanvasElement): void {
    const dpr = window.devicePixelRatio || 1;
    canvas.width = W * dpr;
    canvas.height = H * dpr;
    canvas.style.width = `${W}px`;
    canvas.style.height = `${H}px`;
    const ctx = canvas.getContext('2d');
    if (ctx) ctx.scale(dpr, dpr);
  }

  private resetState(): void {
    this.state = {
      ballX: W / 2,
      ballY: H / 2,
      ballDx: BASE_SPEED * randomSign(),
      ballDy: BASE_SPEED,
      playerX: (W - PADDLE_W) / 2,
      aiX: (W - PADDLE_W) / 2,
      playerScore: 0,
      aiScore: 0
    };
  }

  private attachInputHandlers(canvas: HTMLCanvasElement): void {
    canvas.addEventListener('mousemove', (e: MouseEvent) => {
      const rect = canvas.getBoundingClientRect();
      const scaleX = W / rect.width;
      const x = (e.clientX - rect.left) * scaleX;
      this.state.playerX = Math.max(0, Math.min(W - PADDLE_W, x - PADDLE_W / 2));
    });

    canvas.addEventListener(
      'touchmove',
      (e: TouchEvent) => {
        e.preventDefault();
        const rect = canvas.getBoundingClientRect();
        const scaleX = W / rect.width;
        const x = (e.touches[0].clientX - rect.left) * scaleX;
        this.state.playerX = Math.max(0, Math.min(W - PADDLE_W, x - PADDLE_W / 2));
      },
      { passive: false }
    );
  }

  private loop(canvas: HTMLCanvasElement): void {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    this.update();
    this.draw(ctx);

    this.rafId = requestAnimationFrame(() => this.loop(canvas));
  }

  private update(): void {
    const s = this.state;

    s.ballX += s.ballDx;
    s.ballY += s.ballDy;

    // Side wall bounces
    if (s.ballX - BALL_R < 0) {
      s.ballX = BALL_R;
      s.ballDx = Math.abs(s.ballDx);
    }
    if (s.ballX + BALL_R > W) {
      s.ballX = W - BALL_R;
      s.ballDx = -Math.abs(s.ballDx);
    }

    // AI paddle tracking
    const aiCenter = s.aiX + PADDLE_W / 2;
    if (s.ballX > aiCenter + 2) {
      s.aiX = Math.min(W - PADDLE_W, s.aiX + AI_SPEED);
    } else if (s.ballX < aiCenter - 2) {
      s.aiX = Math.max(0, s.aiX - AI_SPEED);
    }

    // Player paddle collision (bottom)
    const playerY = H - PADDLE_MARGIN - PADDLE_H;
    if (
      s.ballDy > 0 &&
      s.ballY + BALL_R >= playerY &&
      s.ballY + BALL_R <= playerY + PADDLE_H + 4 &&
      s.ballX >= s.playerX &&
      s.ballX <= s.playerX + PADDLE_W
    ) {
      s.ballY = playerY - BALL_R;
      const offset = (s.ballX - (s.playerX + PADDLE_W / 2)) / (PADDLE_W / 2);
      s.ballDx = offset * BASE_SPEED * 1.5;
      s.ballDy = -Math.abs(s.ballDy);
    }

    // AI paddle collision (top)
    const aiY = PADDLE_MARGIN;
    if (
      s.ballDy < 0 &&
      s.ballY - BALL_R <= aiY + PADDLE_H &&
      s.ballY - BALL_R >= aiY - 4 &&
      s.ballX >= s.aiX &&
      s.ballX <= s.aiX + PADDLE_W
    ) {
      s.ballY = aiY + PADDLE_H + BALL_R;
      const offset = (s.ballX - (s.aiX + PADDLE_W / 2)) / (PADDLE_W / 2);
      s.ballDx = offset * BASE_SPEED * 1.5;
      s.ballDy = Math.abs(s.ballDy);
    }

    // Scoring
    if (s.ballY - BALL_R < 0) {
      s.playerScore++;
      this.resetBall(1);
    }
    if (s.ballY + BALL_R > H) {
      s.aiScore++;
      this.resetBall(-1);
    }
  }

  private resetBall(dirY: 1 | -1): void {
    this.state.ballX = W / 2;
    this.state.ballY = H / 2;
    this.state.ballDx = BASE_SPEED * randomSign();
    this.state.ballDy = BASE_SPEED * dirY;
  }

  private draw(ctx: CanvasRenderingContext2D): void {
    const s = this.state;

    // Background
    ctx.fillStyle = '#05070a';
    ctx.fillRect(0, 0, W, H);

    // Center dashed divider
    ctx.setLineDash([4, 6]);
    ctx.strokeStyle = 'rgba(102, 242, 255, 0.12)';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(0, H / 2);
    ctx.lineTo(W, H / 2);
    ctx.stroke();
    ctx.setLineDash([]);

    // Scores
    ctx.font = '10px "IBM Plex Mono", monospace';
    ctx.fillStyle = 'rgba(0, 229, 255, 0.5)';
    ctx.textAlign = 'left';
    ctx.fillText(`AI ${s.aiScore}`, 6, PADDLE_MARGIN + PADDLE_H + 14);
    ctx.textAlign = 'right';
    ctx.fillText(`YOU ${s.playerScore}`, W - 6, H - PADDLE_MARGIN - PADDLE_H - 6);

    // AI paddle (top)
    ctx.shadowColor = '#00e5ff';
    ctx.shadowBlur = 5;
    ctx.fillStyle = '#00e5ff';
    ctx.fillRect(s.aiX, PADDLE_MARGIN, PADDLE_W, PADDLE_H);

    // Player paddle (bottom)
    ctx.shadowColor = '#66f2ff';
    ctx.shadowBlur = 8;
    ctx.fillStyle = '#66f2ff';
    ctx.fillRect(s.playerX, H - PADDLE_MARGIN - PADDLE_H, PADDLE_W, PADDLE_H);

    // Ball
    ctx.beginPath();
    ctx.arc(s.ballX, s.ballY, BALL_R, 0, Math.PI * 2);
    ctx.shadowColor = '#00e5ff';
    ctx.shadowBlur = 14;
    ctx.fillStyle = '#00e5ff';
    ctx.fill();

    ctx.shadowBlur = 0;
  }
}
