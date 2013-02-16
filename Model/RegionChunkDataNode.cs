using System.IO;
using Substrate.Core;
using Substrate.Nbt;

namespace NBTExplorer.Model
{
    public class RegionChunkDataNode : DataNode, IMetaTagContainer
    {
        private RegionFile _regionFile;
        private NbtTree _tree;
        private int _x;
        private int _z;

        private CompoundTagContainer _container;

        public RegionChunkDataNode (RegionFile regionFile, int x, int z)
        {
            _regionFile = regionFile;
            _x = x;
            _z = z;
            _container = new CompoundTagContainer(new TagNodeCompound());
        }

        protected override NodeCapabilities Capabilities
        {
            get
            {
                return NodeCapabilities.CreateTag
                    | NodeCapabilities.PasteInto
                    | NodeCapabilities.Search;
            }
        }

        public override bool HasUnexpandedChildren
        {
            get { return !IsExpanded; }
        }

        public override string NodeDisplay
        {
            get { return "Chunk [" + _x + ", " + _z + "]"; }
        }

        protected override void ExpandCore ()
        {
            if (_tree == null) {
                _tree = new NbtTree();
                _tree.ReadFrom(_regionFile.GetChunkDataInputStream(_x, _z));

                if (_tree.Root != null)
                    _container = new CompoundTagContainer(_tree.Root);
            }

            foreach (TagNode tag in _tree.Root.Values) {
                TagDataNode node = TagDataNode.CreateFromTag(tag);
                if (node != null)
                    Nodes.Add(node);
            }
        }

        protected override void ReleaseCore ()
        {
            _tree = null;
            Nodes.Clear();
        }

        protected override void SaveCore ()
        {
            using (Stream str = _regionFile.GetChunkDataOutputStream(_x, _z)) {
                _tree.WriteTo(str);
            }
        }

        public bool IsNamedContainer
        {
            get { return true; }
        }

        public bool IsOrderedContainer
        {
            get { return false; }
        }

        public INamedTagContainer NamedTagContainer
        {
            get { return _container; }
        }

        public IOrderedTagContainer OrderedTagContainer
        {
            get { return null; }
        }

        public int TagCount
        {
            get { return _container.TagCount; }
        }

        public bool DeleteTag (TagNode tag)
        {
            return _container.DeleteTag(tag);
        }
    }
}
