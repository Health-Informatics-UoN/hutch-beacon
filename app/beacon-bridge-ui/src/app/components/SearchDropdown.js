export default function SearchDropdown({id, label, options, onChange}) {
  return (
    <div className="flex flex-wrap space-x-2 mb-4 items-baseline">
      <label htmlFor={id}>{label}</label>
      <select
        id={id}
        onChange={(event) => {
          onChange(event.target.value)}
        }
        className="rounded-lg bg-transparent border-2 border-uon-blue-60 border-solid py-2 grow max-w-full"
        defaultValue={""}
      >
        <option value="" disabled></option>
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
