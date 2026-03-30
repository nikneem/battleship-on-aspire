import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { type RadarContact } from '../../landing-page.models';

@Component({
  selector: 'bat-landing-radar',
  imports: [NgOptimizedImage],
  templateUrl: './landing-radar.html',
  styleUrl: './landing-radar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LandingRadar {
  readonly contacts = input<readonly RadarContact[]>([]);
}
