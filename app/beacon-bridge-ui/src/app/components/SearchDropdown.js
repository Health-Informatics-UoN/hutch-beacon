export default function SearchDropdown({id, label, options, onChange}) {
  return (
    <div className="space-x-2">
      <label htmlFor={id}>{label}</label>
      <select
        id={id}
        onChange={(event) => {
          onChange(event.target.value)}
        }
        className="bg-transparent border-2 border-white border-solid"
      >
        {
          options.map((opt, key)=>{
            let name = `${opt.id} - ${opt.label}`;
            return <option key={key} value={opt.id}>{name}</option>
          })
        }
      </select>
    </div>
  )
}
