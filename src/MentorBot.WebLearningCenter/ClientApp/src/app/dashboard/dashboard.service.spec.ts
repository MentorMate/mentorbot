import { createHttpFactory, HttpMethod, SpectatorHttp } from '@ngneat/spectator/jest';

import { firstValueFrom } from 'rxjs';

import { DashboardService } from './dashboard.service';
import { MessagesStatistic } from './dashboard.models';

describe('DashboardService', () => {
  let spectator: SpectatorHttp<DashboardService>;
  const createHttp = createHttpFactory(DashboardService);

  beforeEach(() => (spectator = createHttp()));

  it('should return expected data (HttpClient called once)', async () => {
    const expected: MessagesStatistic[] = [
      { count: 10, probabilityPercentage: 1 },
      { count: 5, probabilityPercentage: 0 },
    ];

    const resTask = firstValueFrom(spectator.service.getData());
    spectator.expectOne('get-messages-stats', HttpMethod.GET).flush(expected);

    const data = await resTask;
    expect(data).toEqual(expected);
  });
});
