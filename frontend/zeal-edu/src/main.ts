// ============================================================
// Project   : EProject 3 - Zeal Education
// Author    : Lê Chính Trực - Student1557161 (C2403L0751)
// Email     : letruc.work@gmail.com - truc.lc.2427@aptechlearning.edu.vn
// Created   : 2026-19-04
// Course    : ADSE - Aptech Vietnam (https://aptechvietnam.com.vn)
// License   : All rights reserved. Unauthorized use prohibited.
// ============================================================

import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from '@app/app.config';
import { App } from '@app/app';

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
