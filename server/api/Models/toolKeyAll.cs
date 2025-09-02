using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models;

public partial class ToolKeyAll
{
    public int tool_key_id { get; set; }

    public int? type_id { get; set; }

    public int? tool_id { get; set; }

    public int? type_ref_id { get; set; }

    public int? tool_ref_id { get; set; }

    public int? size_ref_id { get; set; }

    public int? original_spec { get; set; }

    public int? ref_spec { get; set; }
    public int? knurling { get; set; }
    public int? source_original_key_id { get; set; }

    public int? source_ref_key_id { get; set; }

    public byte[]? pic_before_hst { get; set; }

    [NotMapped]
    public string? image_url { get; set; }

    public string? pic_before_hst_file_name { get; set; }

    public virtual SizeRef? SizeRef { get; set; }

    public virtual ToolKeyOriginal? SourceOriginalKey { get; set; }

    public virtual ToolKeyReference? SourceRefKey { get; set; }

    public virtual Tool? Tool { get; set; }

    public virtual ICollection<ToolMachineMap> ToolMachineMaps { get; set; } = new List<ToolMachineMap>();

    public virtual ICollection<ToolPadMap> ToolPadMaps { get; set; } = new List<ToolPadMap>();

    public virtual Tool? ToolRef { get; set; }

    public virtual TypeModel? Type { get; set; }

    public virtual TypeModel? TypeRef { get; set; }
}
