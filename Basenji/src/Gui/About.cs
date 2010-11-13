// About.cs
// 
// Copyright (C) 2008 Patrick Ulbrich
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Text;
using Gtk;
using Gdk;

namespace Basenji.Gui
{
	public partial class About : AboutDialog
	{
		private static readonly string		subTitle	= S._("A portable volume indexing tool");
		private static readonly string		dbVersion	= string.Format(S._("Using VolumeDB v{0}."), Util.GetVolumeDBVersion());
		private static readonly string		comments	= string.Format("{0}\n{1}", subTitle, dbVersion);
		private static readonly string		copyright	= string.Format("{0}{1}", S._("Copyright (c) "), App.Copyright);
		
		private static readonly string[]	authors		= new string[] {
@"Maintainer:
Patrick Ulbrich <zulu99@gmx.net>

Contributors:
Francesco Marella (https://launchpad.net/~francesco-marella)
Mauro Solcia (https://launchpad.net/~maurosolcia)
" };
		
		private static readonly string		translatorCredits	= 
@"Brazilian Portuguese
  André Gondim (https://launchpad.net/~andregondim)
  Gilberto Martins (https://launchpad.net/~gsilva-martins)
  Gustavo Guidorizzi (https://launchpad.net/~gguido)
  Matheus de Araújo (https://launchpad.net/~suetamac)

Bulgarian:
  svilborg (https://launchpad.net/~svilborg)

Danish:
  nanker (https://launchpad.net/~nanker)

Estonian:
  olavi tohver (https://launchpad.net/~olts16)

French:
  Anthony Guéchoum (https://launchpad.net/~athael)
  Pierre Slamich (https://launchpad.net/~pierre-slamich)
  Stephane Ricci (https://launchpad.net/~stephane-ricci)

German:
  Patrick Ulbrich (https://launchpad.net/~pulb)

Hebrew:
  Yaron (https://launchpad.net/~sh-yaron)

Hungarian:
  Polesz (https://launchpad.net/~polesz-nedudu)
  Roti (https://launchpad.net/~roti-al)

Italian:
  Davide Vidal (https://launchpad.net/~davide-vidal)
  Francesco Marella (https://launchpad.net/~francesco-marella)
  Giuseppe Caliendo (https://launchpad.net/~giuseppe-caliendo)
  Guybrush88 (https://launchpad.net/~erpizzo)
  meltingshell (https://launchpad.net/~meltingshell)

Occitan (post 1500):
  Cédric Valmary (https://launchpad.net/~cvalmary)

Polish:
  Stanisław Chmiela (https://launchpad.net/~chmiela-st)
  XeonBloomfield (https://launchpad.net/~xeonbloomfield)

Russian:
  Alexander 'FONTER' Zinin (https://launchpad.net/~spore-09)
  Dmitri Konoplev (https://launchpad.net/~knoplef)
  Eugene Marshal (https://launchpad.net/~lowrider)
  Nikolai Romanik (https://launchpad.net/~arhey)
  Papazu (https://launchpad.net/~pavel-z)
  Sciko (https://launchpad.net/~tempsciko-gmail)
  Sergey Sedov (https://launchpad.net/~serg-sedov)
  Vladimir (https://launchpad.net/~bonza-land)

Spanish:
  Daniel Garcia Stelzner (https://launchpad.net/~dani-garcia87)
  DiegoJ (https://launchpad.net/~diegojromerolopez)
  Emiliano (https://launchpad.net/~emilianohfernandez)
  Feder Sáiz (https://launchpad.net/~federsaiz)
  Jonay Santana (https://launchpad.net/~jonay-santana)
  Jorge Dardón (https://launchpad.net/~jdardon)
  martin (https://launchpad.net/~martu)
  Martín V. (https://launchpad.net/~martinvukovic)
  monkey (https://launchpad.net/~monkey-libre)  
  Nicolás M. Zahlut (https://launchpad.net/~nzahlut)
  Paco Molinero (https://launchpad.net/~franciscomol)
  Rommel Anatoli Quintanilla Cruz (https://launchpad.net/~romeluko1100)
  victor tejada yau (https://launchpad.net/~victormtyau)

Turkish:
  Cihan Ersoy (https://launchpad.net/~cihan.ersoy)";
		
		public About() {
			// general window settings
			if (Gui.Base.WindowBase.MainWindow != null)
				this.TransientFor = Gui.Base.WindowBase.MainWindow;
			
			this.Modal = true;
			SkipTaskbarHint	= true;
			
			Icon = Basenji.Icons.Icon.Stock_About.Render(this, IconSize.Menu);
			
			// about dialog settings
			Logo				= new Pixbuf(App.APP_DATA_PATH + "/basenji.svg", 200, 200);
			ProgramName			= App.Name;
			Version				= App.Version;
			Comments			= comments;
			Copyright			= copyright;
			Website				= "http://www.launchpad.net/basenji";
			//WebsiteLabel		= "Basenji Homepage";
			Authors				= authors;
			TranslatorCredits	= translatorCredits;
		}
	}	
}
