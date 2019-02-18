import { Selector, t } from 'testcafe';

export default class DashBoard {
  constructor() {
    //New service dialogue box
    this.homeButton = Selector("img").withAttribute("title", "startside");
    this.profileButton = Selector("ul > li > a > i").withExactText("AutoTest");
    this.logoutButton = Selector(".dropdown-item").withText("Logg ut");
    this.newServiceButton = Selector("button > span").withExactText("ny tjeneste");
    this.serviceSearch = Selector("#service-search");
    this.tjenesteEier = Selector("#service-owner");
    this.tjenesteNavn = Selector("#service-name");
    this.lagringsNavn = Selector("#service-saved-name");
    this.rettigheterMelding = Selector("p").withText("Du har ikke rettigheter");
    this.opprettButton = Selector("button").withExactText("Opprett");
  }

  async logout() {
    await t
      .hover(this.homeButton)
      .click(this.logoutButton)
  }

  async createNewService(serviceName) {
    //New service button and dialogue selectors
  }
}