﻿using FileRenamer.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using TagLib;
using FileRenamer.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;


namespace FileRenamer.Operations {
    public static class GeneralOperations {

        const string _module = "GeneralOperations";

        public static string GetConnectionString( ) {
            string connectionStringName = ConfigurationManager.AppSettings["connectionstringName"];
            var connStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            var connectionString = connStringSettings.ConnectionString;
            return connectionString;
        }

        public static FolderpathInfo GetFolderpathInfo( string folderPath, List<Author> authors ) {
            var fpi = new FolderpathInfo( ) { FilePath = folderPath };
            /*  split filepath into n fields: 
                0 is the drive, 
                1 is "Books"
                n-1 is "10 Minute Slices"
            */
            try {
                var first = string.Empty;
                var last = string.Empty;
                var fields = folderPath.Split( '\\' );
                var i = fields.Length;
                var audiobookTitle = string.Empty;
                switch( fields.Length ) {
                    case 5:
                    case 8:
                        //field 3/7 is the author first and last delimited by ", "
                        //field 4/8 is the title
                        i = i - 3;
                        first = fields[i].Split( ',' )[1].Trim( );
                        last = fields[i].Split( ',' )[0].Trim( );
                        fpi.Author = authors.Find( x => x.Last == last && x.First == first );
                        if( fpi.Author == null ) {
                            fpi.Author = new Author( ) { First = first, Last = last };
                        }
                        if( fields[3].Contains( "-" ) ) {
                            audiobookTitle = fields[i + 1].Split( '-' )[1].Trim( );
                        } else {
                            audiobookTitle = fields[i + 1];
                        }
                        break;
                    case 4:
                    case 7:
                        //field 3/7 is author and title delimited by " - "
                        i = i - 2;
                        var author = fields[i].Split( '-' )[0].Trim( );
                        first = author.Split( ',' )[1].Trim( );
                        last = author.Split( ',' )[0].Trim( );
                        fpi.Author = authors.Find( x => x.Last == last && x.First == first );
                        if( fpi.Author == null ) {
                            fpi.Author = new Author( ) { First = first, Last = last };
                        }
                        audiobookTitle = fields[i].Split( '-' )[1].Trim( );
                        break;
                }
                if( fpi.Author.AuthorId > 0 ) {
                    fpi.Author = AuthorOperations.Author_Get( fpi.Author.AuthorId );
                }
                if( fpi.Author.AuthorId > 0 ) {
                    var authorAudiobooks = AudiobookOperation.Audiobook_GetListByAuthor( fpi.Author.AuthorId );
                    fpi.Audiobook = authorAudiobooks.Find( x => x.Title == audiobookTitle );
                    if( fpi.Audiobook == null ) {
                        fpi.Audiobook = new Audiobook( ) {
                            Title = audiobookTitle,
                            AuthorId = fpi.Author.AuthorId
                        };
                    }
                }
            }
            catch( Exception ex ) {
                var caption = string.Format( "{0}.GetFolderpathInfo( folderPath, authors )", _module );
                GeneralOperations.WriteToLogFile( string.Format( "Error in {0}: {1}", caption, ex.Message ) );
                ShowMessageDialog( "An Error Occurred", caption, ex.Message );
            }
            return fpi;
        }

        public static string TargetFilename( string folderPath, string fileBaseName, int count, uint fileNumber, string numberFormat ) {
            var di = new DirectoryInfo( folderPath );
            return string.Format( "{0}\\{1}_{2}_{3}.mp3", di.FullName, fileBaseName, count.ToString( ), fileNumber.ToString( numberFormat ) );
        }

        public static void SaveFile( TagInfo tagInfo ) {
            var firstYear = 2018;
            var yearString = DateTime.Now.Year == firstYear ? "" : string.Format( "{0}, ", DateTime.Now.Year );

            if( new FileInfo( tagInfo.FilePath ).Exists ) {
                var file = TagLib.File.Create( tagInfo.FilePath );
                file.Tag.Title = tagInfo.Title;
                file.Tag.Album = tagInfo.Album;
                file.Tag.Performers = tagInfo.Performers;
                file.Tag.AlbumArtists = tagInfo.AlbumArtists;
                file.Tag.Track = tagInfo.TrackNumber;
                file.Tag.Genres = tagInfo.Genres;
                file.Tag.Comment = string.Format(
                    "{0} Application, Copyright {2}{3} {1} Michael Rubin, All rights reserved",
                    System.Windows.Forms.Application.ProductName,
                    yearString,
                    firstYear,
                    ( (char)169 ).ToString( ));
                file.Save( );
            } else {
                throw new Exception( string.Format( "Filepath {0} does not exist", tagInfo.FilePath ) );
            }
        }

        //logging
        public static void WriteToLogFile( string message ) {
            var logFilePath = string.Format( "{0}\\{1}", new DirectoryInfo( Path.GetDirectoryName( Application.ExecutablePath ) ).Parent.Parent.FullName, "logfile.txt" );
            using( StreamWriter w = System.IO.File.AppendText( logFilePath ) ) {
                w.WriteLine( string.Format( "{0}: {1}", DateTime.Now.ToString( "yyyyMMddHHmmss" ), message ) );
            }
        }

        // message dialog
        public static void ShowMessageDialog(string caption, string title, string detail ) {
            var frmMessage = new FrmMessage( caption, title, detail );
            frmMessage.ShowDialog( );

        }

    }
}
