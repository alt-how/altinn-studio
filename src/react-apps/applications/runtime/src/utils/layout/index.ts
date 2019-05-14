import { ILayoutComponent, ILayoutContainer } from '../../features/form/layout/types';

export function getLayoutComponentById(id: string, layout: [ILayoutComponent | ILayoutContainer]): ILayoutComponent {
  const component: ILayoutComponent = layout.find((element) => element.id === id) as ILayoutComponent;
  return component;
}
