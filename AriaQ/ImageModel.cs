//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AriaQ
{
    using System;
    using System.Collections.Generic;
    
    public partial class ImageModel
    {
        public long ID { get; set; }
        public Nullable<long> PatientSer { get; set; }
        public string PatientId { get; set; }
        public string MachineId { get; set; }
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public string ImageId { get; set; }
        public Nullable<System.DateTime> AcquisitionDate { get; set; }
        public string ImageType { get; set; }
        public string ImageStatus { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovalDate { get; set; }
        public string ImageNote { get; set; }
        public string Comment { get; set; }
        public Nullable<long> Oncologist { get; set; }
        public Nullable<double> IsocenterX { get; set; }
        public Nullable<double> IsocenterY { get; set; }
        public Nullable<double> IsocenterZ { get; set; }
        public string DicomUID { get; set; }
        public Nullable<int> Energy { get; set; }
        public Nullable<double> MetersetExposure { get; set; }
        public Nullable<int> ExposureTime { get; set; }
        public Nullable<int> XRayTubeCurrent { get; set; }
        public string PrimaryDosimeterUnit { get; set; }
        public Nullable<int> ImageNotesLen { get; set; }
        public string CreationUserName { get; set; }
        public Nullable<long> CourseSer { get; set; }
        public Nullable<long> RadiationSer { get; set; }
        public Nullable<long> StudySer { get; set; }
        public Nullable<long> SeriesSer { get; set; }
        public Nullable<long> ImageSer { get; set; }
        public Nullable<long> SliceSer { get; set; }
        public Nullable<long> ResourceSer { get; set; }
    }
}
