# SimpleUploader
This is the Windows program (C#) I wrote to provide users upload services to our site.

Our clients, professional photographers, needed a way to simply upload a set of photos from their
computers to our web site.  Using a web browser to upload was not practical for the following reasons:

  * Their photos were often organized into separate folders and it wasn't practical to upload each
    folder separately
  * Some of these photo sets could take many hours to upload and browser just weren't robust enough
    to handle this.
  * In some cases we could to preprocessing on the client side to shrink the images allowing a much
    faster upload time.
    
This was originally written years ago using SOAP as the transport layer.  Since web uploads have become
more stable and reliably, it was refactored a year ago to use web protocols to tranfer the photos.  We
used Perforce as our revision control system until recently, but chose not to do the work involved in
bringing the revision history into Git. 
