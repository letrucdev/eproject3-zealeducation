import { ActivatedRouteSnapshot } from '@angular/router';

export interface Breadcrumb {
  label: string;
  url: string;
}

export function buildBreadcrumbs(root: ActivatedRouteSnapshot): Breadcrumb[] {
  const trail: Breadcrumb[] = [];
  const segments: string[] = [];
  let node: ActivatedRouteSnapshot | null = root;

  while (node) {
    for (const segment of node.url) {
      segments.push(segment.path);
    }

    const label = node.data['breadcrumb'];
    if (typeof label === 'string' && label.length > 0) {
      trail.push({ label, url: '/' + segments.join('/') });
    }

    node = node.firstChild;
  }

  return trail;
}
