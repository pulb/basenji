// TagLibMetadataProvider.cs
//
// Copyright (C) 2011, 2012 Patrick Ulbrich
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
using System.Collections.Generic;
using System.Globalization;
using Platform.Common.Diagnostics;
using TagLib;

namespace VolumeDB.Metadata
{

	public sealed class TagLibMetadataProvider : MetadataProvider
	{
		// enable detailed debugging messages
		private const bool VERBOSE = true;
		
		// orientation strings used by libextractor 0.5.x, too
		private readonly Dictionary<TagLib.Image.ImageOrientation, string> orientations 
		= new Dictionary<TagLib.Image.ImageOrientation, string>() {
			{ TagLib.Image.ImageOrientation.BottomLeft, 	"bottom, left"	},
			{ TagLib.Image.ImageOrientation.BottomRight, 	"bottom, right"	},
			{ TagLib.Image.ImageOrientation.LeftBottom, 	"left, bottom"	},
			{ TagLib.Image.ImageOrientation.LeftTop, 		"left, top"		},
			{ TagLib.Image.ImageOrientation.RightBottom, 	"right, bottom"	},
			{ TagLib.Image.ImageOrientation.RightTop,		"right, top"	},
			{ TagLib.Image.ImageOrientation.TopLeft,		"top, left"		},
			{ TagLib.Image.ImageOrientation.TopRight,		"top, right"	}
		};
		
		private static string[] formatTypes = new string[] {
			"Video",
			"Audio",
			"Image"
		};
		
		private static readonly NumberFormatInfo numformat = CultureInfo.InvariantCulture.NumberFormat;
		
		public TagLibMetadataProvider () {
			
		}
		
		public override IEnumerable<MetadataItem> GetMetadata(string filename, string mimetype) {
			EnsureNotDisposed();
			
			if ((mimetype != null) && !FileTypes.AvailableTypes.ContainsKey(mimetype)) {
				
				if (VERBOSE && Global.EnableDebugging)
					Debug.WriteLine("taglib# does not like files of type " + mimetype);
				
				return null;
			}
			
			TagLib.File f;
			
			try {
				f = File.Create(filename, mimetype, ReadStyle.Average); // null mimetype allowed
			} catch (UnsupportedFormatException) {
				if (VERBOSE && Global.EnableDebugging)
					Debug.WriteLine("taglib# does not like files of type " + mimetype);
				return null;
			
			} catch (CorruptFileException) {
				if (VERBOSE && Global.EnableDebugging)
					Debug.WriteLine(string.Format("taglib# says file '{0}' is broken.", filename));
				throw; // make the scanner output the error
				// return null;
			}
			
			List<MetadataItem> metadata = new List<MetadataItem>();
			
			AddGenericTags(metadata, f);
			AddCustomTags(metadata, f);
			AddProperties(metadata, f);
			
			if (metadata.Count == 0)
				return null;
			
			return metadata;
		}
		
		private static void AddGenericTags(List<MetadataItem> metadata, TagLib.File f) {
			if (!AddData(metadata, MetadataType.ARTIST, f.Tag.JoinedPerformers))
				AddData(metadata, MetadataType.ARTIST, f.Tag.JoinedAlbumArtists);
			
			if (f.Tag.Year > 0)
				AddData(metadata, MetadataType.YEAR, f.Tag.Year.ToString());
			
			AddData(metadata, MetadataType.ALBUM,		f.Tag.Album);
			AddData(metadata, MetadataType.TITLE,		f.Tag.Title);
			AddData(metadata, MetadataType.GENRE,		f.Tag.JoinedGenres);
			AddData(metadata, MetadataType.COPYRIGHT,	f.Tag.Copyright);
			AddData(metadata, MetadataType.COMMENT,		f.Tag.Comment);
			AddData(metadata, MetadataType.LYRICS,		f.Tag.Lyrics);
		}
		
		private void AddCustomTags(List<MetadataItem> metadata, TagLib.File f) {
			
			// if the file is an image, extract image specific tags.
			// all metadata should use the libextractor 0.5.x format in order to
			// preserve compatibility.
			
			if (f is TagLib.Image.File) {
				TagLib.Image.ImageTag tag = (TagLib.Image.ImageTag)f.Tag;

				AddData(metadata, MetadataType.CAMERA_MAKE,		tag.Make);
				AddData(metadata, MetadataType.CAMERA_MODEL,	tag.Model);
				AddData(metadata, MetadataType.CREATOR,			tag.Creator);
				//AddData(metadata, MetadataType.SOFTWARE,		tag.Software); // returns "1.0" for pictures !?
				
				if (tag.DateTime.HasValue)
					AddData(metadata, MetadataType.DATE, 
					        tag.DateTime.Value.ToString("yyyy:MM:dd HH:mm:ss")); // ":" in date intended
				
				// exposure times are generally very short, 
				// so allow at least 3 decimal places
				if (tag.ExposureTime.HasValue)
					AddData(metadata, MetadataType.EXPOSURE, 
					        tag.ExposureTime.Value.ToString("#0.000 s", numformat));
				
				if (tag.FocalLength.HasValue)
					AddData(metadata, MetadataType.FOCAL_LENGTH, tag.FocalLength.Value.ToString("#0.0 mm", numformat));
				
				if (tag.FocalLengthIn35mmFilm.HasValue)
					AddData(metadata, MetadataType.FOCAL_LENGTH_35MM, tag.FocalLengthIn35mmFilm.Value.ToString("#0.0 mm", numformat));
				
				if (tag.Orientation != TagLib.Image.ImageOrientation.None)
					AddData(metadata, MetadataType.ORIENTATION, orientations[tag.Orientation]);
				
				if (tag.ISOSpeedRatings.HasValue)
					AddData(metadata, MetadataType.ISO_SPEED, tag.ISOSpeedRatings.Value.ToString());
			}
			
		}
		
		private static void AddProperties(List<MetadataItem> metadata, TagLib.File f) {
			
			if (f.Properties == null)
				return;
			
			if (f.Properties.MediaTypes != MediaTypes.None) {
				TimeSpan duration = f.Properties.Duration;
				
				if (duration.Ticks > 0) {
					AddData(metadata, MetadataType.DURATION, 
					       MetadataUtils.SecsToMetadataDuration(duration.TotalSeconds));
				}
			}
			
			string[] formats = new string[3];
			int fmtCount = 0;
			int w = int.MinValue, h = int.MinValue;
			int q = int.MinValue;
			
			foreach (ICodec codec in f.Properties.Codecs) {
				
				if ((codec.MediaTypes & MediaTypes.Video) == TagLib.MediaTypes.Video) {
					IVideoCodec vcodec = codec as IVideoCodec;
					w = vcodec.VideoWidth;
					h = vcodec.VideoHeight;
					
					if (HasValidData(vcodec.Description)) {
						formats[0] = vcodec.Description;
						fmtCount++;
					}
				}
				
				if ((codec.MediaTypes & MediaTypes.Audio) == TagLib.MediaTypes.Audio) {
					IAudioCodec acodec = codec as IAudioCodec;
					
					if (HasValidData(acodec.Description)) {
						
						StringBuilder fmt = new StringBuilder();
						
						fmt.Append(acodec.Description);
						
						if (acodec.AudioBitrate > 0) {
							if (fmt.Length > 0)
								fmt.Append(", ");
							
							fmt.Append(acodec.AudioBitrate.ToString()).Append(" kb/s");
						}
						
						if (acodec.AudioChannels > 0) {
							if (fmt.Length > 0)
								fmt.Append(", ");
							
							fmt.Append(acodec.AudioChannels.ToString()).Append(" channels");
						}
						
						if (acodec.AudioSampleRate > 0) {
							if (fmt.Length > 0)
								fmt.Append(", ");
							
							fmt.Append(acodec.AudioSampleRate.ToString()).Append(" Hz");
						}
						
						if (fmt.Length > 0) {
							formats[1] = fmt.ToString();
							fmtCount++;
						}
					}
				}
				
				if ((codec.MediaTypes & MediaTypes.Photo) == TagLib.MediaTypes.Photo) {
					IPhotoCodec pcodec = codec as IPhotoCodec;
					
					// don't overwrite video dimensions
					if ((w == int.MinValue) && (h == int.MinValue)) {
						w = pcodec.PhotoWidth;
						h = pcodec.PhotoHeight;
					}
					
					q = pcodec.PhotoQuality;
					
					if (HasValidData(pcodec.Description)) {
						formats[2] = pcodec.Description;
						fmtCount++;
					}
				}
			}
			
			// size format of libextrator is NxN
			if ((w > int.MinValue) && (h > int.MinValue)) 
				AddData(metadata, MetadataType.SIZE, string.Format("{0}x{1}", w, h));
			
			if (q > int.MinValue)
				AddData(metadata, MetadataType.IMAGE_QUALITY, q.ToString());
			
			
			// build format string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < formats.Length; i++) {
				string s = formats[i];
				
				if (s == null) 
					continue;
				
				if (sb.Length > 0)
					sb.Append ("; ");
				
				if (fmtCount > 1)
					sb.AppendFormat("{0}: {1}", formatTypes[i], s);
				else
					sb.Append(s);
			}
			
			if (sb.Length > 0)
				AddData(metadata, MetadataType.FORMAT, sb.ToString());
		}
		
		private static bool AddData(List<MetadataItem> metadata, MetadataType type, string data) {
			if (!HasValidData(data))
				return false;
			
			metadata.Add(new MetadataItem(type, data));
			
			if (VERBOSE && Global.EnableDebugging)
				Debug.WriteLine(string.Format("Got new metadata from taglib#: {0} - {1}", type, data));
					
			return true;
		}
		
		private static bool HasValidData(string s) {
			if (string.IsNullOrEmpty(s))
				return false;
			
			bool allWhite = true;
			
			for (int i = 0; i < s.Length; i++) {
				if (!char.IsWhiteSpace(s[i])) {
					allWhite = false;
					break;
				}
			}
			
			return !allWhite;
		}
	}
}
