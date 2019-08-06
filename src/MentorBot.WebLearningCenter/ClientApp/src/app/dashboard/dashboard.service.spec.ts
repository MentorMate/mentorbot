import { DashboardService } from "./dashboard.service";
import { MessagesStatistic } from './dashboard.models';
import { of } from "rxjs";

describe('DashboardService', () => {
  let httpClientSpy: { get: jasmine.Spy };
  let dashboardService: DashboardService;

  beforeEach(() => {
    // TODO: spy on other methods too
    httpClientSpy = jasmine.createSpyObj('HttpClient', ['get']);
    dashboardService = new DashboardService(<any>httpClientSpy);
  });

  it('should return expected data (HttpClient called once)', done=> {
    const expected: MessagesStatistic[] =
      [{ count: 10, probabilityPercentage: 1 }, { count: 5, probabilityPercentage: 0 }];

    httpClientSpy.get.and.returnValue(of(expected));

    dashboardService.getData().subscribe(
      data => {
        expect(data).toEqual(expected, 'expected data');
        expect(httpClientSpy.get.calls.count()).toBe(1, 'one call');
        done();
      });
  });
});
