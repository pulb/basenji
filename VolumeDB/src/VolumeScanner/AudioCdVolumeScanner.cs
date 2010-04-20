// AudioCdVolumeScanner.cs
// 
// Copyright (C) 2010 Patrick Ulbrich
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
using System.Collections.Generic;
using PlatformIO = Platform.Common.IO;
using MusicBrainz;
using LibExtractor;

namespace VolumeDB.VolumeScanner
{
	public sealed class AudioCdVolumeScanner 
		: AbstractVolumeScanner<AudioCdVolume, AudioCdVolumeInfo, AudioCdScannerOptions>
	{
		private const string MIME_TYPE_AUDIO_TRACK = "audio/x-wav";
		private const string PRESELECTED_CATEGORY = "Music";
		
		// note:
		// do not allow to modify the constuctor parameters 
		// (i.e. database, options)
		// through public properties later, since the scanner 
		// may already use them after scanning has been started,
		// and some stuff has been initialized depending on the 
		// options in the ctor already.
		public AudioCdVolumeScanner(Platform.Common.IO.DriveInfo drive,
		                            VolumeDatabase database,
		                            AudioCdScannerOptions options)
			: base(drive, database, options)
		{
			if (!drive.HasAudioCdVolume)
				throw new ArgumentException("No audio cd present in drive");
			
		}
		
		internal override void ScanningThreadMain(PlatformIO.DriveInfo drive,
		                                          AudioCdVolume volume,
		                                          BufferedVolumeItemWriter writer) {
			
			if (Options.ComputeHashs) {
				SendScannerWarning(S._("Hashcode generation not implemented for audio cds yet."));
			
				volume.IsHashed = false;
			}
			
			AudioCdRootVolumeItem root = GetNewVolumeItem<AudioCdRootVolumeItem>(VolumeDatabase.ID_NONE,
			                                                                     "/",
			                                                                     null,
			                                                                     null,
			                                                                     VolumeItemType.AudioCdRootVolumeItem);
			
			LocalDisc localdisc = LocalDisc.GetFromDevice(drive.Device);
			
			if (localdisc == null)
				throw new ApplicationException("Could not read contents of the audio cd");
			
			TimeSpan[] durations = localdisc.GetTrackDurations();
			List<AudioTrackVolumeItem> items = new List<AudioTrackVolumeItem>();
			for (int i = 0; i < durations.Length; i++) {
				AudioTrackVolumeItem item = GetNewVolumeItem<AudioTrackVolumeItem>(root.ItemID,
				                                                                   "Track " + (i + 1),
				                                                                   MIME_TYPE_AUDIO_TRACK,
				                                                                   null,
				                                                                   VolumeItemType.AudioTrackVolumeItem);
				item.SetAudioTrackVolumeItemFields(durations[i]);
				
				items.Add(item);
				
				VolumeInfo.Tracks++;
				VolumeInfo.Duration = VolumeInfo.Duration.Add(durations[i]);
			}
			
			// retrieve musicbrainz metadata
			// (the metadata field of AudioTrackVolumeItems is set 
			// depending on the EnableMusicBrainz flag)
			if (Options.EnableMusicBrainz) {
				
				try {
					// may throw MusicBrainzNotFoundException
					Release release = Release.Query(localdisc).PerfectMatch();

					if (release == null) {
						SendScannerWarning(S._("No MusicBrainz metadata available for this disc."));
					} else {
						var tracks = release.GetTracks();
						
						if (tracks.Count != items.Count) {
							SendScannerWarning(S._("The trackcount retrieved from MusicBrainz does not match the trackcount of the local disc. Skipped."));
						} else {
							string albumTitle = release.GetTitle();
							int releaseYear = GetReleaseYear(release);
							
							for(int i = 0; i < tracks.Count; i++) {							
								items[i].Name = tracks[i].GetTitle();
								items[i].MetaData = GetMetaData(tracks[i], albumTitle, releaseYear);
							}
							
							volume.Title = albumTitle;
							
							// preset category
							ReleaseType rtype = release.GetReleaseType();
							if (rtype == ReleaseType.Album || 
							    rtype == ReleaseType.EP ||
							    rtype == ReleaseType.Compilation ||
							    rtype == ReleaseType.Remix) {
								volume.Category = PRESELECTED_CATEGORY;
							}
						}
					}
				} catch (MusicBrainzNotFoundException) {
					SendScannerWarning(S._("Error connecting to MusicBrainz server."));
				}
			}
			
			volume.SetAudioCdVolumeFields(VolumeInfo.Tracks, VolumeInfo.Duration);
			
			// write items
			if (this.HasDB) {
				writer.Write(root);
				
				foreach (AudioTrackVolumeItem item in items) {
					writer.Write(item);
				}
			}
		}

		private static string GetMetaData(Track track,
		                                  string albumTitle,
		                                  int releaseYear) {
			
			List<Keyword> keywords = new List<Keyword>();
			
			if (!string.IsNullOrEmpty(albumTitle)) {
				keywords.Add(new Keyword {
					keywordType = KeywordType.EXTRACTOR_ALBUM,
					keyword = albumTitle
				});
			}
			
			string artistName = track.GetArtist().GetName();
			if (!string.IsNullOrEmpty(artistName)) {
				keywords.Add(new Keyword {
					keywordType = KeywordType.EXTRACTOR_ARTIST,
					keyword = artistName
				});
			}
			
			string title = track.GetTitle();
			if (!string.IsNullOrEmpty(title)) {
				keywords.Add(new Keyword {
					keywordType = KeywordType.EXTRACTOR_TITLE,
					keyword = title
				});
			}
			
			if (releaseYear > 0) {
				keywords.Add(new Keyword {
					keywordType = KeywordType.EXTRACTOR_YEAR,
					keyword = releaseYear.ToString()
				});
			}
			
			return MetaDataHelper.PackExtractorKeywords(keywords.ToArray());
		}
		
		private static int GetReleaseYear(Release release) {
			int releaseYear = int.MaxValue;
			
			foreach (Event e in release.GetEvents()) {
				string date = e.Date;
				if (date != null) {
					try {
						int y = int.Parse(date.Substring(0, 4));
						if (y < releaseYear)
							releaseYear = y;
					} catch {}
				}
			}
			
			return (releaseYear == int.MaxValue) ? 0 : releaseYear;
		}
	}
}
