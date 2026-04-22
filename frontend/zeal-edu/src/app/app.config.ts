import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { OVERLAY_DEFAULT_CONFIG } from '@angular/cdk/overlay';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTanStackQuery, QueryClient } from '@tanstack/angular-query-experimental';

import { routes } from './app.routes';
import { authInterceptor } from './core/http/auth-interceptor';
import { authPersistenceInterceptor } from './core/http/auth-persistence-interceptor';
import { baseUrlInterceptor } from './core/http/base-url-interceptor';
import { errorInterceptor } from './core/http/error-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([
        baseUrlInterceptor,
        authInterceptor,
        authPersistenceInterceptor,
        errorInterceptor,
      ]),
    ),
    provideTanStackQuery(new QueryClient()),
    { provide: OVERLAY_DEFAULT_CONFIG, useValue: { usePopover: false } },
  ],
};
